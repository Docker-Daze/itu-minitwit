using Microsoft.AspNetCore.Mvc;

namespace minitwit.web;

public class RerouteController : Controller
{
    [HttpGet("")]
    public IActionResult ToPublic()
    {
        return LocalRedirect("/public");
    }
}