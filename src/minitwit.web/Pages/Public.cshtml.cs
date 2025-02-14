using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;
using minitwit.web.Pages;

namespace itu_minitwit.Pages;

[IgnoreAntiforgeryToken]
public class PublicModel : PageModel
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    public List<MessageDTO> Messages { get; set; }
    public int _currentPage;
    [BindProperty]
    public MessageInputModel MessageInput { get; set; }
    public PublicModel(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }
    
    public async Task<ActionResult> OnGet([FromQuery] int? page)
    {
        _currentPage = page ?? 1;
        Messages = await _messageRepository.GetMessages(_currentPage);
        return Page();
    }
    
    public async Task<ActionResult> OnPost()
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
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

        await _messageRepository.AddMessage(userId, message);
        return Redirect("/public");
    }



    public async Task<ActionResult> OnPostAddMessageAsync(string message){

        await Task.CompletedTask;

        if(message.Length < 1){

            return BadRequest("Message cannot be empty");
        }

        if(User.Identity.IsAuthenticated){

            string userID = await _userRepository.GetUserID(User.Identity.Name);

            if(userID != null){
                await _messageRepository.AddMessage(userID, message);
                return RedirectToPage("/public");
            }
            else{
                return BadRequest("User with no ID");
            }
            
        }

        return Unauthorized();
    }
}


