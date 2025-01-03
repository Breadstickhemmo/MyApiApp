using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace MyApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordService _passwordService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, PasswordService passwordService, TokenService tokenService, ILogger<AuthController> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register(UserRegisterRequest userRequest)
        {
            try
            {
                if (_context.Users.Any(u => u.Username == userRequest.Username))
                {
                    _logger.LogInformation("Пользователь с таким именем уже существует.");
                    return BadRequest("Пользователь с таким именем уже существует.");
                }

                string salt;
                string passwordHash = _passwordService.HashPassword(userRequest.Password, out salt);

                var user = new User
                {
                    Username = userRequest.Username,
                    PasswordHash = passwordHash,
                    Salt = salt
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                user.Token = _tokenService.GenerateToken(user.Username, user.Id);

                HttpContext.Session.SetString("User Id", user.Id.ToString());

                return Ok(new { Token = user.Token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при регистрации пользователя: {Message}", ex.Message);
                return StatusCode(500, "Произошла ошибка при регистрации пользователя.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(u => u.Username == loginRequest.Username);
                if (user == null || !_passwordService.VerifyPassword(loginRequest.Password, user.PasswordHash, user.Salt))
                {
                    return Unauthorized("Неправильный логин или пароль.");
                }

                user.Token = _tokenService.GenerateToken(user.Username, user.Id);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("User Id", user.Id.ToString());

                return Ok(new { Token = user.Token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при входе в систему.");
                return StatusCode(500, "Произошла ошибка при входе в систему.");
            }
        }

        [HttpPatch("password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = _context.Users.Find(userId);
                if (user == null || !_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.Salt))
                {
                    return Unauthorized("Неправильный текущий пароль.");
                }

                string newSalt;
                user.PasswordHash = _passwordService.HashPassword(request.NewPassword, out newSalt);
                user.Salt = newSalt;

                user.Token = _tokenService.GenerateToken(user.Username, user.Id);

                _context.SaveChanges();

                return Ok(new { Token = user.Token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при изменении пароля.");
                return StatusCode(500, "Произошла ошибка при изменении пароля.");
            }
        }
    }
}
