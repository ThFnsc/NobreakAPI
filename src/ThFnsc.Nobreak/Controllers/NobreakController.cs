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
public class NobreakController : ControllerBase, IActionFilter
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
    public ActionResult<NobreakStatus> Test([Required]string @for)
    {
        if (@for.Equals("untilflat", StringComparison.InvariantCultureIgnoreCase))
            return _nobreakCommunicator.TestUntilFlatBattery();
        else if (@for.Equals("quick", StringComparison.InvariantCultureIgnoreCase))
            return _nobreakCommunicator.Test();
        else if(byte.TryParse(@for, out var minutes) && minutes is > 0 and < 100)
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
    /// Unused
    /// </summary>
    [NonAction]
    public void OnActionExecuting(ActionExecutingContext context) { }

    /// <summary>
    /// Rewrites result if an exception happened to a 503 page
    /// </summary>
    [NonAction]
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null)
        {
            context.HttpContext.RequestServices
                .GetRequiredService<ILogger<NobreakController>>()
                .LogError(context.Exception, "Action error");
            context.ExceptionHandled = true;
            context.Result = Problem(title: "Nobreak unavailable", detail: context.Exception.Message, statusCode: 503);
        }
    }
}
