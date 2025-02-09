using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace itu_minitwit.Pages;

public class UserTimelineModel : PageModel
{
    private readonly IMessageRepository _messageRepository;
    public int _currentPage;
    public List<MessageDTO> Messages { get; set; }

    public UserTimelineModel(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }
    
    public async Task<ActionResult> OnGet(string user, [FromQuery] int? page)
    {
        _currentPage = page ?? 1;
        Messages = await _messageRepository.GetMessagesUserTimeline(user, _currentPage);
        return Page();
    }
}