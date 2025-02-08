using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace itu_minitwit.Pages;

public class PublicModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();

    public PublicModel(IUserRepository userRepository, IMessageRepository messageRepository)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }
    
    public async Task<ActionResult> OnGet()
    {
        Messages = await _messageRepository.GetMessages();
        return Page();
    }
}