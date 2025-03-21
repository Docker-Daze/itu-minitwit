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

    public PublicModel(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<ActionResult> OnGet([FromQuery] int? page)
    {
        _currentPage = page ?? 1;
        Messages = await _messageRepository.GetMessages(_currentPage).ConfigureAwait(false);
        return Page();
    }
}