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
    
    // GET and POST messages
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

    // POST for register
    /*[HttpPost("/api/register")]
    public async Task<IActionResult> PostRegister([FromBody] MessageRequest request)
    {
        var user = Activator.CreateInstance<User>();

        user.UserName = Input.UserName;
                
        var existingUser = await _userManager.FindByEmailAsync(Input.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "Email address already exists.");
            return Page();
        }
                
        await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, Input.Password);
                
        if (result.Succeeded)
        {
            user.GravatarURL = await _userRepository.GetGravatarURL(Input.Email, 80);
                    
            var claim = new Claim("User Name", Input.UserName);
            await _userManager.AddClaimAsync(user, claim);

            await _signInManager.SignInAsync(user, isPersistent: false);
            HttpContext.Session.SetString("UserId", user.Id);
                
            return LocalRedirect(returnUrl);
        }
        
        return Ok(new { message = "Registration posted successfully" });
    }*/
}