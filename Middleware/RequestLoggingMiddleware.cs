using System.Security.Claims;
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering(); 

        using (var scope = context.RequestServices.CreateScope())
        {
            var historyDbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();

            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                string bodyContent = string.Empty;

                if (context.Request.Method != HttpMethods.Get)
                {
                    try
                    {
                        context.Request.Body.Position = 0;
                        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                        {
                            bodyContent = await reader.ReadToEndAsync();
                            context.Request.Body.Position = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при чтении тела запроса.");
                    }
                }

                var historyRecord = new History
                {
                    UserId = userId,
                    HttpMethod = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "",
                    Timestamp = DateTime.UtcNow,
                    BodyContent = bodyContent
                };

                try
                {
                    historyDbContext.History.Add(historyRecord);
                    await historyDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении данных в истории запросов.");
                }
            }
        }

        await _next(context);
    }
}