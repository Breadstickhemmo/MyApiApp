using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestHistoryController : ControllerBase
    {
        private readonly HistoryDbContext _historyDbContext;
        private readonly ILogger<RequestHistoryController> _logger;

        public RequestHistoryController(HistoryDbContext historyDbContext, ILogger<RequestHistoryController> logger)
        {
            _historyDbContext = historyDbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetRequestHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
                {
                    var history = await _historyDbContext.History
                        .Where(r => r.UserId == userId)
                        .ToListAsync();

                    return Ok(history);
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении истории запросов.");
                return StatusCode(500, "Произошла ошибка при получении истории запросов.");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRequestHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
                {
                    var userHistory = _historyDbContext.History
                        .Where(r => r.UserId == userId);

                    _historyDbContext.History.RemoveRange(userHistory);
                    await _historyDbContext.SaveChangesAsync();

                    return NoContent();
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении истории запросов.");
                return StatusCode(500, "Произошла ошибка при удалении истории запросов.");
            }
        }
    }
}