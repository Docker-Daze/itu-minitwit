using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using minitwit.core;
using minitwit.infrastructure;
using minitwit.web.Pages;
using Serilog;

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

    private readonly MetricsService _metricsService;
    
    
    private static readonly List<Message> _pendingMessages = new();
    private static readonly object _batchLock = new();
    private static readonly List<Follower> _pendingFollows = new();
    private static readonly object _batchLockFollow = new();
    private static readonly List<Follower> _pendingUnFollows = new();
    private static readonly object _batchLockUnFollow = new();
    private const int BatchSize = 10;

    public ApiController(IMessageRepository messageRepository, IUserRepository userRepository, UserManager<User> userManager,
        IUserStore<User> userStore, SignInManager<User> signInManager, MetricsService metricsService)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;

        _userManager = userManager;
        _userStore = userStore;
        _emailStore = (IUserEmailStore<User>)_userStore;
        _signInManager = signInManager;

        _userRepository = userRepository;
        _metricsService = metricsService;
    }

    public IActionResult? NotReqFromSimulator(HttpContext context)
    {
        var fromSimulator = context.Request.Headers["Authorization"];
        if (fromSimulator != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                status = 403,
                error_msg = "You are not authorized to use this resource!"
            });
        }
        return null;
    }

    // GET for latest
    [HttpGet("/api/latest")]
    public async Task<IActionResult> Latest()
    {
        return await Task.FromResult(Ok(new { latest = _latest }));
    }

    // POST for register
    [HttpPost("/api/register")]
    public async Task<IActionResult> PostRegister([FromBody] RegisterRequest request, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            using (_metricsService.MeasureRequestRegisterDuration())
            {
                _latest = latest;
                var user = Activator.CreateInstance<User>();

                user.UserName = request.username;

                var existingUser = await _userManager.FindByEmailAsync(request.email);
                if (existingUser != null || user.UserName == null)
                {
                    ModelState.AddModelError(string.Empty, "Email address already exists.");
                    return BadRequest(ModelState);
                }

                await _userStore.SetUserNameAsync(user, request.username, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, request.email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, request.pwd);
                _metricsService.IncrementRegisterCounter();
                if (result.Succeeded)
                {
                    user.GravatarURL = await _userRepository.GetGravatarURL(request.email, 80);

                    var claim = new Claim("User Name", request.username);
                    await _userManager.AddClaimAsync(user, claim);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    HttpContext.Session.SetString("UserId", user.Id);

                    return NoContent();
                }
                
                return LocalRedirect("/api/register");
            }
        }
    }

    // GET and POST messages
    // GET specified user latest messages.
    [HttpGet("/api/msgs/{username}")]
    public async Task<IActionResult> GetUserMsgs(string username, [FromQuery] int no, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            _metricsService.IncrementGetRequestsCounter();
            _latest = latest;

            var notFromSimResponse = NotReqFromSimulator(HttpContext);
            if (notFromSimResponse != null)
            {
                return notFromSimResponse;
            }

            if (await _userRepository.GetUserID(username) == null)
            {
                Log.Warning("there was no user with name: {username}", username);
                return NotFound();
            }

            var messages = await _messageRepository.GetMessagesFromUsernameSpecifiedAmount(username, no);
            return Ok(messages);
        }
    }

    // GET latest messages. Doesn't matter who posted.
    [HttpGet("/api/msgs")]
    public async Task<IActionResult> GetMsgs([FromQuery] int no, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            _metricsService.IncrementGetRequestsCounter();
            _latest = latest;

            var notFromSimResponse = NotReqFromSimulator(HttpContext);
            if (notFromSimResponse != null)
            {
                return notFromSimResponse;
            }

            var messages = await _messageRepository.GetMessagesSpecifiedAmount(no);
            return Ok(messages);
        }
    }

    // POST a message. Author is the {username}.
    [HttpPost("/api/msgs/{username}")]
    public async Task<IActionResult> PostMsgs(string username, [FromBody] MessageRequest request, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            using (_metricsService.MeasureRequestPostMsgsDuration())
            {
                _latest = latest;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized();
                }

                var user = await _userRepository.GetUserFromUsername(username);
                if (user == null)
                {
                    Log.Warning("there was no user with name: {username}", username);
                    return NotFound();
                }

                var message = request.Content;
                var flagged = request.flagged;
                try
                {
                    Message theMessage = await _messageRepository.AddMessage(user, message, flagged);
                    _metricsService.IncrementPostMsgsCounter();
                    lock (_batchLock)
                    {
                        _pendingMessages.Add(theMessage);

                        if (_pendingMessages.Count < BatchSize)
                        {
                            // Not enough to flush yet
                            return NoContent();
                        }

                        // Swap out the batch and clear buffer
                        var batch = _pendingMessages.ToList();
                        _pendingMessages.Clear();

                        // Fire‐and‐forget the batch save
                        _ = _messageRepository.AddMessagesBatchAsync(batch);
                    }

                    return NoContent();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Could not add the message: {message} to the database", message);
                    return NoContent();
                }

            }
        }
    }

    // POST and GET for follow.
    // GET for follow. Gets json on who it follows
    [HttpGet("/api/fllws/{username}")]
    public async Task<IActionResult> GetFollow(string username, [FromQuery] int no, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            _metricsService.IncrementGetRequestsCounter();
            _latest = latest;
            

            var notFromSimResponse = NotReqFromSimulator(HttpContext);
            if (notFromSimResponse != null)
            {
                return notFromSimResponse;
            }
            if (await _userRepository.GetUserFromUsername(username) == null)
            {
                Log.Warning("there was no user with name: {username}", username);
                return NotFound();
            }

            var followers = await _userRepository.GetFollowers(username);
            return Ok(new { follows = followers.Select(f => f.follows).ToList() });
        }
    }

    // Get endpoint for healthcheck, used by nginx reverse-proxy and rolling updates.
    [HttpGet("/api/health")]
    public IActionResult HealthCheck(){
        return Ok();
    }

    // POST for follow and unfolow. {Username} is the person who will follow/unfollow someone.
    [HttpPost("/api/fllws/{username}")]
    public async Task<IActionResult> PostFollow(string username, [FromBody] FollowRequest request, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            _latest = latest;
            if (request.follow != null)
            {
                using (_metricsService.MeasureRequestFollowDuration())
                {
                    _metricsService.IncrementFollowCounter();
                    try
                    {
                        Follower theRequest = await _userRepository.FollowUser(username, request.follow);
                        lock (_batchLockFollow)
                        {
                            _pendingFollows.Add(theRequest);

                            if (_pendingFollows.Count < BatchSize)
                            {
                                // Not enough to flush yet
                                return NoContent();
                            }

                            // Swap out the batch and clear buffer
                            var batch = _pendingFollows.ToList();
                            _pendingFollows.Clear();

                            // Fire‐and‐forget the batch save
                            _ = _userRepository.AddFollowersBatchAsync(batch);
                        }

                        return NoContent();
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "User {User} tried to follow {Target} but something went wrong", username, request.follow);
                        return NoContent();
                    }
                }
            }

            using (_metricsService.MeasureRequestUnfollowDuration())
            {
                _metricsService.IncrementUnFollowCounter();
                try
                {
                    Follower theRequest = await _userRepository.UnfollowUser(username, request.unfollow!);
                    lock (_batchLockUnFollow)
                    {
                        _pendingUnFollows.Add(theRequest);

                        if (_pendingUnFollows.Count < BatchSize)
                        {
                            // Not enough to flush yet
                            return NoContent();
                        }

                        // Swap out the batch and clear buffer
                        var batch = _pendingUnFollows.ToList();
                        _pendingUnFollows.Clear();

                        // Fire‐and‐forget the batch save
                        _ = _userRepository.RemoveFollowersBatchAsync(batch);
                    }

                    return NoContent();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "User {User} tried to unfollow {Target} but something went wrong", username, request.unfollow);
                    return NoContent();
                }
            }
        }
    }
}