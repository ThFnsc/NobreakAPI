using Microsoft.AspNetCore.Mvc.Filters;

namespace ThFnsc.Nobreak;

/// <summary>
/// Filter that shuts down the app if many repeated errors occur
/// </summary>
public class ShutdownAppOnManyErrorsAttribute : ActionFilterAttribute
{
    private readonly int _errorsBeforeShutdown;
    private int _errorCount = 0;

    /// <summary>
    /// Filter that shuts down the app if many repeated errors occur.
    /// </summary>
    /// <param name="errorsBeforeShutdown">How many actions must throw an error in sequence before shutdown</param>
    public ShutdownAppOnManyErrorsAttribute(int errorsBeforeShutdown)
    {
        _errorsBeforeShutdown = errorsBeforeShutdown;
    }

    /// <summary>
    /// Checks here if the counter must be incremented (and app terminated) or reset.
    /// </summary>
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is null)
            _errorCount = 0;
        else if (++_errorCount >= _errorsBeforeShutdown)
        {
            context.HttpContext.RequestServices
                .GetRequiredService<ILogger<ShutdownAppOnManyErrorsAttribute>>()
                .LogCritical("Maximum exception count of {MaxErrors} reached. Shutting down application...", _errorsBeforeShutdown);
            Environment.Exit(1);
        }
    }
}
