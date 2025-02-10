using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;
using minitwit.web.Pages;

namespace itu_minitwit.Pages;

public class PublicModel : PageModel
{
    private readonly IMessageRepository _messageRepository;
    public List<MessageDTO> Messages { get; set; }
    public int _currentPage;
    [BindProperty]
    public MessageInputModel MessageInput { get; set; }
    public PublicModel(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }
    
    public async Task<ActionResult> OnGet([FromQuery] int? page)
    {
        _currentPage = page ?? 1;
        Messages = await _messageRepository.GetMessages(_currentPage);
        return Page();
    }
    
    public async Task<ActionResult> OnPost()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
        var message = MessageInput.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError("Message", "Message cannot be empty.");
        }
        else if (message.Length > 160)
        {
            ModelState.AddModelError("Message", "Message cannot be more 160 characters.");
        }
        if (!ModelState.IsValid)
        {
            Messages = await _messageRepository.GetMessages(_currentPage);
            return Page();
        }

        await _messageRepository.AddMessage(User.Identity.Name, message);
        return Redirect("/public");
    }
}