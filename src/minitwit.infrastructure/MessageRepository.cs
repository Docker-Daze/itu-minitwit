using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MessageRepository : IMessageRepository
{
    private readonly MinitwitDbContext _dbContext;
    private IUserRepository _userRepository;
    private const int PerPage = 10;

    private readonly IDbContextFactory<MinitwitDbContext> _factory;

    public MessageRepository(MinitwitDbContext dbContext, IUserRepository userRepository, IDbContextFactory<MinitwitDbContext> factory)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _factory = factory;
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

        var query = (from message in _dbContext.Messages
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
        var query = (from message in _dbContext.Messages
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

        var query = (from message in _dbContext.Messages
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

        var query = (from message in _dbContext.Messages
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

        var following = await _dbContext.Followers
            .Where(f => f.WhoId == userId)
            .Select(f => f.WhomId)
            .ToListAsync();

        var query = (from message in _dbContext.Messages
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
            await using var ctx = _factory.CreateDbContext();

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