using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using minitwit.core;

namespace minitwit.infrastructure;

public class MessageRepository : IMessageRepository
{
    private IUserRepository _userRepository;
    private const int PerPage = 10;

    private readonly IServiceScopeFactory _scopeFactory;

    public MessageRepository(IUserRepository userRepository, IServiceScopeFactory scopeFactory)
    {
        _userRepository = userRepository;
        _scopeFactory = scopeFactory;
    }

    public Task<Message> AddMessage(User user, string message, int flagged = 0)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message text cannot be empty.");
        }
        if (message.Length > 160)
        {
            throw new ArgumentException("Message text cannot exceed 160 characters.");
        }

        Message newMessage = new()
        {
            Text = message,
            PubDate = DateTime.UtcNow,
            User = user,
            Flagged = flagged
        };
        return Task.FromResult(newMessage);
    }

    public async Task<List<MessageDTO>> GetMessages(int page)
    {
        int offset = (page - 1) * PerPage;
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
        var query = (from message in ctx.Messages
                     orderby message.PubDate descending
                     where message.Flagged == 0
                     select new MessageDTO
                     {
                         Text = message.Text!,
                         Username = message.User!.UserName!,
                         PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                         GravatarUrl = message.User.GravatarURL
                     }).Skip(offset).Take(PerPage);

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<APIMessageDTO>> GetMessagesSpecifiedAmount(int amount)
    {
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
        var query = (from message in ctx.Messages
                     orderby message.PubDate descending
                     where message.Flagged == 0
                     select new APIMessageDTO
                     {
                         content = message.Text!,
                         user = message.User!.UserName!,
                         pub_date = ((DateTimeOffset)message.PubDate).ToUnixTimeSeconds(),
                     }).Take(amount);

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<MessageDTO>> GetMessagesUserTimeline(string username, int page)
    {
        int offset = (page - 1) * PerPage;
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
        var query = (from message in ctx.Messages
                     orderby message.PubDate descending
                     where message.Flagged == 0 && message.User!.UserName == username
                     select new MessageDTO
                     {
                         Text = message.Text!,
                         Username = message.User!.UserName!,
                         PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                         GravatarUrl = message.User.GravatarURL
                     }).Skip(offset).Take(PerPage);

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<APIMessageDTO>> GetMessagesFromUsernameSpecifiedAmount(string username, int amount)
    {
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
        var query = (from message in ctx.Messages
                     orderby message.PubDate descending
                     where message.Flagged == 0 && message.User!.UserName == username
                     select new APIMessageDTO
                     {
                         content = message.Text!,
                         user = message.User!.UserName!,
                         pub_date = ((DateTimeOffset)message.PubDate).ToUnixTimeSeconds(),
                     }).Take(amount);

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<MessageDTO>> GetMessagesOwnTimeline(string username, int page)
    {
        int offset = (page - 1) * PerPage;

        var userId = await _userRepository.GetUserID(username);
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
        var following = await ctx.Followers
            .Where(f => f.WhoId == userId)
            .Select(f => f.WhomId)
            .ToListAsync();

        var query = (from message in ctx.Messages
                     orderby message.PubDate descending
                     where message.Flagged == 0
                           && (message.User!.UserName == username || following.Contains(message.User!.Id))
                     select new MessageDTO
                     {
                         Text = message.Text!,
                         Username = message.User!.UserName!,
                         PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                         GravatarUrl = message.User.GravatarURL
                     }).Skip(offset).Take(PerPage);

        var result = await query.ToListAsync();
        return result;
    }

    public async Task AddMessagesBatchAsync(IEnumerable<Message> messages)
    {
        try
        {
            // create a fresh, scoped context just for this batch
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();

            // if you still have navigations on your Message, mark them Unchanged:
            foreach (var msg in messages)
                if (msg.User is not null)
                    ctx.Entry(msg.User).State = EntityState.Unchanged;

            await ctx.Messages.AddRangeAsync(messages);
            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}