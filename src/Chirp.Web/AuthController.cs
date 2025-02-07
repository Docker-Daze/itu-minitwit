using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
namespace Chirp.Web;

public class AuthController : Controller
{
    [Route("login")]
    public IActionResult Login()
    {
        // This will trigger the GitHub OAuth flow
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "GitHub");
    }
    
    [Route("logout")]
    [HttpPost]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync();
        return LocalRedirect(returnUrl);
    }
}