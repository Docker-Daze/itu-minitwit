using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;
using minitwit.web.Pages;

namespace itu_minitwit.Pages;

[IgnoreAntiforgeryToken]
public class UserTimelineModel : PageModel
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private int _currentPage;
    public List<MessageDTO>? Messages { get; set; }

    public UserTimelineModel(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }
    
    public async Task<ActionResult> OnGet(string user, [FromQuery] int? page)
    {
        _currentPage = page ?? 1;

        Messages = await _messageRepository.GetMessagesUserTimeline(user, _currentPage);
        return Page();
    }

    public async Task<bool> IsFollowing(string who){
        if(User.Identity!.Name != null){
            return await _userRepository.IsFollowing(User.Identity.Name, who);
        }
        else return false;
    }

    public async Task<IActionResult> OnPostFollowAsync(string user)
    {
        await Task.CompletedTask;

        if(string.IsNullOrEmpty(user))
        {
            return BadRequest("User or acions cannot be null");
        }

        if(User.Identity?.IsAuthenticated != true){
            return Unauthorized();
        }

        if(await _userRepository.IsFollowing(User.Identity.Name!, user)){
            return RedirectToPage("/public");
        }

        await _userRepository.FollowUser(User.Identity.Name!, user);
        TempData["FlashMessage"] = $"You are now following \"{user}\"";
        return RedirectToPage("/UserTimeline", new { user = user });
    } 

    public async Task<IActionResult> OnPostUnFollowAsync(string user)
    {
        await Task.CompletedTask;

        if(string.IsNullOrEmpty(user))
        {
            return BadRequest("User or acions cannot be null");
        }

        if(User.Identity?.IsAuthenticated != true){
            return Unauthorized();
        }

        await _userRepository.UnfollowUser(User.Identity.Name!, user);
        TempData["FlashMessage"] = $"You are no longer following \"{user}\"";
        return RedirectToPage("/UserTimeline", new { user = user });
    } 

}