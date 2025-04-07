using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace itu_minitwit.Pages;

[IgnoreAntiforgeryToken]
public class Follow : PageModel
{
    private readonly IUserRepository _userRepository;
    
    [BindProperty(SupportsGet = true)]
    public string? user {get; set;}
    
    public Follow(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<ActionResult> OnGet()
    {
        if (User.Identity?.Name == null)
        { 
            return Unauthorized();
        }

        await _userRepository.FollowUser(User.Identity.Name, user!);
        TempData["FlashMessage"] = $"You are now following \"{user}\"";
        return Redirect($"/{user}");
    }
}