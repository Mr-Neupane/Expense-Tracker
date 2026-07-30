using Microsoft.AspNetCore.Mvc.Filters;

namespace ExpenseTracker.Middlewares;

public class LoggingExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<LoggingExceptionFilter> _logger;

    public LoggingExceptionFilter(ILogger<LoggingExceptionFilter> logger)
    {
        _logger = logger;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var controller = context.RouteData.Values["controller"];
        var action = context.RouteData.Values["action"];

        _logger.LogError(context.Exception,
            "Unhandled exception in {Controller}.{Action}",
            controller, action);

        return Task.CompletedTask;
    }
}
