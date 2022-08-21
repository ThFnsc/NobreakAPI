using Microsoft.AspNetCore.Mvc;

namespace ThFnsc.Nobreak.Controllers;

[ApiController]
[Route("/")]
[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Redirect() =>
        RedirectPermanent("/Swagger");
}
