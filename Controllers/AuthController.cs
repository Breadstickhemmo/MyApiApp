using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyApiApp.Data;
using MyApiApp.Models;
using MyApiApp.Services;

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
        public IActionResult Register(User user)
        {
            try
            {
                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    return BadRequest("Пользователь с таким именем уже существует.");
                }

                user.PasswordHash = _passwordService.HashPassword(user.PasswordHash, out string salt);
                user.Salt = salt;

                user.Token = _tokenService.GenerateToken(user.Username, user.Id);

                _context.Users.Add(user);
                _context.SaveChanges();

                HttpContext.Session.SetString("User Id", user.Id.ToString());

                return Ok(new { Token = user.Token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при регистрации пользователя.");
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
                var userId = HttpContext.Session.GetString("User Id");
                if (userId == null)
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = _context.Users.Find(int.Parse(userId));
                if (user == null || !_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.Salt))
                {
                    return Unauthorized("Неправильный текущий пароль.");
                }

                user.PasswordHash = _passwordService.HashPassword(request.NewPassword, out string newSalt);
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
