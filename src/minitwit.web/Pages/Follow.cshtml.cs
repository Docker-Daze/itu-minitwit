using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace itu_minitwit.Pages;

[IgnoreAntiforgeryToken]
public class Follow : PageModel
{
    IUserRepository _userRepository;
    string username;
        public Follow(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<ActionResult> OnGet(string user ,[FromQuery] int? page)
    {
        if(username != null || user != null){
            var response = await _userRepository.FollowUser(username, user);

        if (response == null || response == false)
            return new StatusCodeResult(500);
        }

        return new ContentResult
        {
            Content = $"You are no longer following {user}",
            ContentType = "text/plain",
            StatusCode = 200
        };
    }
}