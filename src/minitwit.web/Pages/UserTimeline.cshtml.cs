using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace itu_minitwit.Pages;

public class UserTimeline : PageModel
{
    private readonly UserManager<User> _userManager;

    public User UserProfile { get; set; }
    
    public UserTimeline(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public ActionResult OnGet(string username)
    {
        return Page();
    }
}