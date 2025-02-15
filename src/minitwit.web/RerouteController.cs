using Microsoft.AspNetCore.Mvc;
using minitwit.core;
using minitwit.infrastructure;

namespace minitwit.web;

public class RerouteController : Controller
{
    private readonly IMessageRepository _messageRepository;
    private int currentPage;
    public RerouteController(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }
    
    [HttpGet("")]
    public IActionResult ToPublic()
    {
        return LocalRedirect("/public");
    }
    

    [HttpGet("/api/msgs/{username}")]
    public async Task<IActionResult> PostMsgs([FromQuery] int? page)
    {
        currentPage = page ?? 1;
        await _messageRepository.GetMessages(currentPage);
        return Ok();
    }
    
    [HttpPost("/api/msgs/{username}")]
    public async Task<IActionResult> GetMsgs([FromBody] MessageRequest request)
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
            return LocalRedirect("/api/msgs");
        }

        await _messageRepository.AddMessage(request.Username, message);
        
        return LocalRedirect("/api/msgs/{username}");
    }
}