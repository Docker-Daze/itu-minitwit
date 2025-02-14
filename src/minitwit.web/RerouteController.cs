using Microsoft.AspNetCore.Mvc;

namespace minitwit.web;

public class RerouteController : Controller
{
    [HttpGet("")]
    public IActionResult ToPublic()
    {
        if (User.Identity.IsAuthenticated)
        {
            LocalRedirect("/" + User.Identity.Name);
        }
        return LocalRedirect("/public");
    }
}