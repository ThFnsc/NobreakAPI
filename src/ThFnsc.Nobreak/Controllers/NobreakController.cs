using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ThFnsc.Nobreak.Core.Models;
using ThFnsc.Nobreak.Core.Services;

namespace ThFnsc.Nobreak.Controllers;
[ApiController]
[Route("[controller]")]
public class NobreakController : ControllerBase
{
    private readonly INobreakCommunicator _nobreakCommunicator;

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
}
