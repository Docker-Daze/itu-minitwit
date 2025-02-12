using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;
using minitwit.infrastructure;
using minitwit.web.Pages;

namespace itu_minitwit.Pages;

public class Follow : PageModel
{
    IUserRepository _userRepository;
    IMessageRepository _messageRepository;
    string username;
        public Follow(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        var username = HttpContext.Session.GetString("UserId");
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