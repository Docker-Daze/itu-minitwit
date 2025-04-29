using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using minitwit.core;
using minitwit.infrastructure;
using minitwit.web.Pages;
using Serilog;

namespace minitwit.web;
[ApiController]
public class ApiController : Controller
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly LatestTracker _tracker;
    
    private readonly MetricsService _metricsService;
    
    private readonly Channel<string[]> _msgChan;
    private readonly Channel<string[]> _followersChan;
    private readonly Channel<string[]> _unFollowersChan;
    private readonly Channel<RegisterRequest> _registerChan;

    public ApiController(IMessageRepository messageRepository, IUserRepository userRepository,
        MetricsService metricsService, Channel<string[]> messageChannel, LatestTracker tracker,
        IFollowChannel followerChannel, IUnfollowChannel unFollowersChannel, IRegisterChannel registerChannel)
    {
        _tracker = tracker;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        
        _metricsService = metricsService;
        _msgChan = messageChannel;
        _followersChan = followerChannel.Channel;
        _unFollowersChan = unFollowersChannel.Channel;
        _registerChan = registerChannel.Channel;
    }

    public IActionResult? NotReqFromSimulator(HttpContext context)
    {
        var fromSimulator = context.Request.Headers.Authorization;
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
        return await Task.FromResult(Ok(new { latest = _tracker.Latest }));
    }

    // POST for register
    [HttpPost("/api/register")]
    public async Task<IActionResult> PostRegister([FromBody] RegisterRequest request, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            using (_metricsService.MeasureRequestRegisterDuration())
            {
                _tracker.Latest = latest;
                _metricsService.IncrementRegisterCounter();
                await _registerChan.Writer.WriteAsync(request);
                return NoContent();
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
            _tracker.Latest = latest;
            
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
            _tracker.Latest = latest;

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
                _tracker.Latest = latest;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized();
                }
                string[] att = { username, request.Content, request.flagged.ToString() };

                _metricsService.IncrementPostMsgsCounter();
                await _msgChan.Writer.WriteAsync(att);
                return NoContent();
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
            _tracker.Latest = latest;


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
    public IActionResult HealthCheck()
    {
        return Ok();
    }

    // POST for follow and unfolow. {Username} is the person who will follow/unfollow someone.
    [HttpPost("/api/fllws/{username}")]
    public async Task<IActionResult> PostFollow(string username, [FromBody] FollowRequest request, [FromQuery] int latest)
    {
        using (_metricsService.MeasureRequestDuration())
        {
            _tracker.Latest = latest;
            if (request.follow != null)
            {
                using (_metricsService.MeasureRequestFollowDuration())
                {
                    _metricsService.IncrementFollowCounter();
                    string[] followRequest = { username, request.follow };
                    await _followersChan.Writer.WriteAsync(followRequest);
                    return NoContent();
                }
            }

            using (_metricsService.MeasureRequestUnfollowDuration())
            {
                _metricsService.IncrementUnFollowCounter();
                string[] unfollowRequest = { username, request.unfollow! };
                await _unFollowersChan.Writer.WriteAsync(unfollowRequest);
                return NoContent();
            }
        }
    }
}