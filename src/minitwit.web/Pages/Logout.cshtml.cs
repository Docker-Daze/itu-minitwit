using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

[IgnoreAntiforgeryToken]
public class LogoutModel : PageModel
{
    private readonly SignInManager<User> _signInManager;

    public LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        TempData["FlashMessage"] = "You were logged out";
        HttpContext.Session.Remove("UserId");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToPage();
    }
}

