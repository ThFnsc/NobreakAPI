using Microsoft.AspNetCore.Mvc;

namespace ThFnsc.Nobreak.Controllers;

/// <summary>
/// Home controller
/// </summary>
[ApiController]
[Route("/")]
[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Redirects the user to Swagger
    /// </summary>
    [HttpGet]
    public IActionResult Redirect() =>
        RedirectPermanent("/Swagger");
}
