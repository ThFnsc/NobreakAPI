using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using ThFnsc.Nobreak.Core.Models;
using ThFnsc.Nobreak.Core.Services;

namespace ThFnsc.Nobreak.Controllers;

/// <summary>
/// Controller to handle nobreak requests
/// </summary>
[ApiController]
[Route("[controller]")]
[ShutdownAppOnManyErrors(4)]
public class NobreakController : ControllerBase, IAsyncActionFilter
{
    private readonly INobreakCommunicator _nobreakCommunicator;

    /// <summary>
    /// Constructor
    /// </summary>
    public NobreakController(INobreakCommunicator nobreakCommunicator)
    {
        _nobreakCommunicator = nobreakCommunicator;
    }

    /// <summary>
    /// Gets the latest status
    /// </summary>
    /// <returns>The latest status</returns>
    [HttpGet(Name = "Status")]
    public NobreakStatus Get() =>
        _nobreakCommunicator.GetStatus();

    /// <summary>
    /// Performs a self-test.
    /// </summary>
    /// <param name="for">
    /// Acceptable values: quick, untilflat or minutes (1-99)
    /// </param>
    /// <returns>The latest status</returns>
    [HttpPost("Test/{for}")]
    public ActionResult<NobreakStatus> Test([Required] string @for)
    {
        if (@for.Equals("untilflat", StringComparison.InvariantCultureIgnoreCase))
            return _nobreakCommunicator.TestUntilFlatBattery();
        else if (@for.Equals("quick", StringComparison.InvariantCultureIgnoreCase))
            return _nobreakCommunicator.Test();
        else if (byte.TryParse(@for, out var minutes) && minutes is > 0 and < 100)
            return _nobreakCommunicator.Test(minutes);
        ModelState.AddModelError(nameof(@for), "Acceptable values are: quick, untilflat or <minutes>(1-99)");
        return ValidationProblem();
    }

    /// <summary>
    /// Changes the beeping mode
    /// </summary>
    /// <param name="state">True to enable beeper</param>
    [HttpPost("Beep/{state}")]
    public NobreakStatus ChangeBeep([Required] bool state) =>
        _nobreakCommunicator.SetBeep(state);

    /// <summary>
    /// Cancels a test
    /// </summary>
    [HttpDelete("Test")]
    public NobreakStatus CancelTest()=>
        _nobreakCommunicator.CancelTest();

    /// <summary>
    /// Handle errors and add a timeout
    /// </summary>
    [NonAction]
    public async Task OnActionExecutionAsync(ActionExecutingContext executingContext, ActionExecutionDelegate next)
    {
        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
        var actionTask = Task.Run(() => next());
        await Task.WhenAny(delayTask, actionTask);
        if (actionTask.IsCompletedSuccessfully)
        {
            var executedContext = actionTask.Result;
            if (executedContext.Exception is not null)
            {
                executedContext.HttpContext.RequestServices
                    .GetRequiredService<ILogger<NobreakController>>()
                    .LogError(executedContext.Exception, "Action error");
                executedContext.ExceptionHandled = true;
                executedContext.Result = Problem(title: "Nobreak unavailable", detail: executedContext.Exception.Message, statusCode: 503);
            }
        }            
        else if (actionTask.IsFaulted)
            throw actionTask.Exception!;
        executingContext.Result = Problem(title: "Nobreak timed out", statusCode: 503);
    }
}
