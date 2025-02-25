using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using minitwit.core;
using minitwit.infrastructure;
using minitwit.web.Pages;

namespace minitwit.web;

public class ApiController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;

    
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private static int _latest = 0;
    
    public ApiController(IMessageRepository messageRepository, IUserRepository userRepository, UserManager<User> userManager,
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
    
    // GET for latest
    [HttpGet("/api/latest")]
    public async Task<IActionResult> Latest()
    {
        return Ok(new { latest = _latest });
    }
    
    // POST for register
    [HttpPost("/api/register")]
    public async Task<IActionResult> PostRegister([FromBody] RegisterRequest request, [FromQuery] int latest)
    {
        Console.WriteLine("HERERERERERE");
        _latest = latest;
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
                
            return StatusCode(204);        
        }

        foreach (var error in result.Errors)
        {
            Console.WriteLine(error.Description);
        }
        
        return LocalRedirect("/api/register");
    }
    
    // GET and POST messages
    // GET specified user latest messages.
    [HttpGet("/api/msgs/{username}")]
    public async Task<IActionResult> GetUserMsgs(string username, [FromQuery] int no, [FromQuery] int latest)
    {
        _latest = latest;
        var messages = await _messageRepository.GetMessagesFromUsernameSpecifiedAmount(username, no);
        return Ok(messages);
    }
    
    // GET latest messages. Doesn't matter who posted.
    [HttpGet("/api/msgs")]
    public async Task<IActionResult> GetMsgs([FromQuery] int no, [FromQuery] int latest)
    {
        _latest = latest;
        var messages = await _messageRepository.GetMessagesSpecifiedAmount(no);
        return Ok(messages);
    }
    
    // POST a message. Author is the {username}.
    [HttpPost("/api/msgs/{username}")]
    public async Task<IActionResult> PostMsgs(string username, [FromBody] MessageRequest request, [FromQuery] int latest)
    {
        _latest = latest;
        if (string.IsNullOrEmpty(username))
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
        
        var userId = await _userRepository.GetUserID(username);
        await _messageRepository.AddMessage(userId, message);
        
        return Ok(new { message = "Message posted successfully" });
    }
    
    // POST and GET for follow.
    // GET for follow. Gets json on who it follows
    [HttpGet("/api/fllws/{username}")]
    public async Task<IActionResult> GetFollow(string username, [FromQuery] int no, [FromQuery] int latest)
    {
        _latest = latest;
        var followers = await _userRepository.GetFollowers(username);
        return Ok(new { follows = followers.Select(f => f.follows).ToList() });
    }

    
    // POST for follow and unfolow. {Username} is the person who will follow/unfollow someone.
    [HttpPost("/api/fllws/{username}")]
    public async Task<IActionResult> PostFollow(string username, [FromBody] FollowRequest request, [FromQuery] int latest)
    {
        _latest = latest;
        if (request.follow != null)
        {
            await _userRepository.FollowUser(username, request.follow);
            return Ok(new { message = "Followed successfully" });
        }
        await _userRepository.UnfollowUser(username, request.unfollow!);
        return Ok(new { message = "Unfollowed successfully" });
    }
    
}