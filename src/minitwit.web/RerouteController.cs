using Microsoft.AspNetCore.Mvc;
using minitwit.core;
using minitwit.infrastructure;

namespace minitwit.web;

public class RerouteController : Controller
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private int currentPage;
    public RerouteController(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }
    
    [HttpGet("")]
    public IActionResult ToPublic()
    {
        return LocalRedirect("/public");
    }
    

    [HttpGet("/api/msgs/{username}")]
    public async Task<IActionResult> GetMsgs(string username, [FromQuery] int no)
    {
        var messages = await _messageRepository.GetMessagesFromUsernameSpecifiedAmount(username, no);
        return Ok(messages);
    }
    
    [HttpPost("/api/msgs/{username}")]
    public async Task<IActionResult> PostMsgs([FromBody] MessageRequest request)
    {

        if (string.IsNullOrEmpty(request.Username))
        {
            return Unauthorized();
        }
        
        var message = request.Content;
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
            return BadRequest(ModelState);
        }
        
        var userId = await _userRepository.GetUserID(request.Username);
        await _messageRepository.AddMessage(userId, message);
        
        return Ok(new { message = "Message posted successfully" });
    }
}