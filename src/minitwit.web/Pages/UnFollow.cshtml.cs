using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace itu_minitwit.Pages;

[IgnoreAntiforgeryToken]
public class UnFollow : PageModel
{
    private readonly IUserRepository _userRepository;
    [BindProperty(SupportsGet = true)]
    public string? user { get; set; }

    public UnFollow(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ActionResult> OnGet()
    {
        if (User.Identity!.IsAuthenticated)
        {
            await _userRepository.UnfollowUser(User.Identity.Name!, user!).ConfigureAwait(false);
            TempData["FlashMessage"] = $"You are no longer following \"{user}\"";
            return Redirect($"/{user}");
        }

        return Unauthorized();
    }
}