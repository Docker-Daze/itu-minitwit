using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace minitwit.web.Pages;

[IgnoreAntiforgeryToken]
public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly UserManager<User> _userManager;

    public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _logger = logger;
        _userManager = userManager;
    }

    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public string Password { get; set; }
    public string ReturnUrl { get; set; }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (User.Identity!.IsAuthenticated)
        {
            ReturnUrl = "/public";
        }

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).ConfigureAwait(false);

        if (returnUrl != null) ReturnUrl = returnUrl;

    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.FindByNameAsync(Username).ConfigureAwait(false);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, false, true).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid password");
            return Page();
        }
        TempData["FlashMessage"] = "You were logged in";
        HttpContext.Session.SetString("UserId", user.Id);
        return LocalRedirect("/public");
    }

    public async Task OnGetAsync(Uri returnUrl = null)
    {
        throw new NotImplementedException();
    }
}