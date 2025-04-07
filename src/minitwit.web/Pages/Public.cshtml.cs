using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace minitwit.web.Pages;

[IgnoreAntiforgeryToken]
public class PublicModel : PageModel
{
    private readonly IMessageRepository _messageRepository;
    public List<MessageDTO> Messages { get; set; }
    public int _currentPage;

    public PublicModel(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        Messages = new List<MessageDTO>();
    }

    public async Task<ActionResult> OnGet([FromQuery] int? page)
    {
        _currentPage = page ?? 1;
        Messages = await _messageRepository.GetMessages(_currentPage);
        return Page();
    }
}