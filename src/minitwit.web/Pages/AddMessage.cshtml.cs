using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;
using minitwit.infrastructure;

namespace itu_minitwit.Pages;

public class AddMessage : PageModel
{
    [Required (ErrorMessage = "Message cannot be empty")]
    [StringLength(160, ErrorMessage = "Maximum length is 160 characters")]
    [Display(Name = "Message Text")]
    public string Message { get; set; }
    public IMessageRepository _messageRepository;
    public IUserRepository _userRepository;
    
    public AddMessage(IMessageRepository messageRepository, IUserRepository userRepository){
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }
    public string Text { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {

        if (string.IsNullOrEmpty(Text))
        {
            ModelState.AddModelError(string.Empty, "Message text cannot be empty.");
            return Page();
        }

        if(!User.Identity.IsAuthenticated){
            return Unauthorized();
        }

        string UserId = await _userRepository.GetUserID(User.Identity.Name);

        if(UserId != null){
            
            await _messageRepository.AddMessage(UserId, Text);

            return new ContentResult{
        
            Content = $"Your message was recorded",
            ContentType = "text/plain",
            StatusCode = 200};
        }

        return BadRequest();
    }
}
