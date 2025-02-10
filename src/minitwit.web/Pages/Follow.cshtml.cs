using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;
using minitwit.infrastructure;
using minitwit.web.Pages;

namespace itu_minitwit.Pages;

public class Follow : PageModel
{
    IUserRepository _userRepository;
    string username;
        public Follow(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        var username = HttpContext.Session.GetString("UserId");
    }
    
    public async Task<ActionResult> OnGet(string user ,[FromQuery] int? page)
    {
        if(username != null){
            await _userRepository.FollowUser(username, user);
            // TODO: Add flash Substitiute
        }

        return Redirect("/");
    }
}