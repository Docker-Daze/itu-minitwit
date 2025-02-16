using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using minitwit.core;
using minitwit.infrastructure;
using minitwit.web.Pages;

namespace minitwit.web;

public class RerouteController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;

    
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private int currentPage;
    public RerouteController(IMessageRepository messageRepository, IUserRepository userRepository, UserManager<User> userManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = (IUserEmailStore<User>)_userStore;
        _signInManager = signInManager;

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
    [HttpPost("/api/register")]
    public async Task<IActionResult> PostRegister([FromBody] RegisterRequest request)
    {
        var user = Activator.CreateInstance<User>();

        user.UserName = request.username;
                
        var existingUser = await _userManager.FindByEmailAsync(request.email);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "Email address already exists.");
            return BadRequest(ModelState);
        }
                
        await _userStore.SetUserNameAsync(user, request.username, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, request.email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, request.pwd);
                
        if (result.Succeeded)
        {
            user.GravatarURL = await _userRepository.GetGravatarURL(request.email, 80);
                    
            var claim = new Claim("User Name", request.username);
            await _userManager.AddClaimAsync(user, claim);

            await _signInManager.SignInAsync(user, isPersistent: false);
            HttpContext.Session.SetString("UserId", user.Id);
                
            return Ok(new { message = "Registration posted successfully" });
        }

        return LocalRedirect("/api/register");
    }
    
    // POST for follow
    [HttpPost("/api/fllws/{username}")]
    public async Task<IActionResult> PostFollow(string username, [FromBody] FollowRequest request)
    {
        await _userRepository.FollowUser(username, request.follow);
        return Ok(new { message = "Followed successfully" });
    }
}