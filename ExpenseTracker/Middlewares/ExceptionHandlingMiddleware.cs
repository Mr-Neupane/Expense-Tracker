namespace ExpenseTracker.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response already started, unable to handle exception");
                throw;
            }

            var errorCode = $"ERR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..24];

            _logger.LogError(ex, "Unhandled exception. ErrorCode: {ErrorCode}", errorCode);

            context.Response.Clear();
            context.Response.Redirect($"/Error?errorCode={errorCode}");
        }
    }
}
