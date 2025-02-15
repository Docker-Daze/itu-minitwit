using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace minitwit.web.Pages;

[IgnoreAntiforgeryToken]
public class TimelineModel : PageModel
{
    private readonly IMessageRepository _messageRepository;
    public int _currentPage;
    public List<MessageDTO> Messages { get; set; }
    [BindProperty] 
    public MessageInputModel MessageInput { get; set; }
    
    public TimelineModel(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }
    
    public async Task<ActionResult> OnGet([FromQuery] int? page)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return LocalRedirect("/public");
        }
        
        _currentPage = page ?? 1;

        Messages = await _messageRepository.GetMessagesOwnTimeline(User.Identity.Name, _currentPage);
        return Page();
    }
}