using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace minitwit.web.Pages;

[IgnoreAntiforgeryToken]
public class AddMessageModel : PageModel
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    [BindProperty]
    public MessageInputModel MessageInput { get; set; } = new() { Text = string.Empty };


    public AddMessageModel(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> OnPost()
    {

        var user = await _userRepository.GetUserFromUsername(User.Identity!.Name!);
        if (user == null)
        {
            return Unauthorized();
        }

        var message = MessageInput.Text;
        if (MessageInput?.Text == null || string.IsNullOrWhiteSpace(MessageInput.Text))
        {
            ModelState.AddModelError(string.Empty, "Message cannot be empty.");
        }
        else if (message.Length > 160)
        {
            ModelState.AddModelError(String.Empty, "Message cannot be more 160 characters.");
        }
        if (!ModelState.IsValid)
        {
            return Redirect("/");
        }

        await _messageRepository.AddMessage(user, message);
        TempData["FlashMessage"] = "Your message was recorded";
        return Redirect("/");
    }
}