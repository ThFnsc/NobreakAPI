using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ThFnsc.Nobreak;

/// <summary>
/// Filter that shuts down the app if many repeated errors occur
/// </summary>
public class ShutdownAppOnManyErrorsAttribute : ResultFilterAttribute
{
    private readonly int _errorsBeforeShutdown;
    private int _errorCount = 0;

    /// <summary>
    /// Filter that shuts down the app if many repeated errors occur.
    /// An error is defined as when <see cref="ResultExecutingContext.Result"/> is an <see cref="IStatusCodeActionResult"/> and <see cref="IStatusCodeActionResult.StatusCode"/> is not within the 2xx range
    /// </summary>
    /// <param name="errorsBeforeShutdown">How many actions must throw an error in sequence before shutdown</param>
    public ShutdownAppOnManyErrorsAttribute(int errorsBeforeShutdown)
    {
        _errorsBeforeShutdown = errorsBeforeShutdown;
    }

    /// <summary>
    /// Checks here if the counter must be incremented (and app terminated) or reset.
    /// </summary>
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        var isr = context.Result is ObjectResult objres;
        if(context.Result is IStatusCodeActionResult codeResult)
        {
            var success = codeResult.StatusCode is null or >= 200 and < 300;
            if (success)
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
}
