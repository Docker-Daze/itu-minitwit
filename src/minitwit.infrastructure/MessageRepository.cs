using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MessageRepository : IMessageRepository
{
    private readonly MinitwitDbContext _dbContext;
    private IUserRepository _userRepository;
    private const int PerPage = 10;
    
    public MessageRepository(MinitwitDbContext dbContext, IUserRepository userRepository)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        
    }
    
    public async Task AddMessage(string userId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message text cannot be empty.");
        }
        if (message.Length > 160)
        {
            throw new ArgumentException("Message text cannot exceed 160 characters.");
        }
        
        var user = await _userRepository.GetUser(userId);

        Message newMessage = new()
        {
            Text = message,
            PubDate = DateTime.Now,
            User = user
        };
        
        await _dbContext.Messages.AddAsync(newMessage); // does not write to the database!
        await _dbContext.SaveChangesAsync(); // persist the changes in the database
    }

    public async Task<List<MessageDTO>> GetMessages(int page)
    {
        int offset = (page - 1) * PerPage;
        
        var query = (from message in _dbContext.Messages
            orderby message.PubDate descending
            where message.Flagged == 0
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.User.UserName,
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                GravatarUrl = message.User.GravatarURL
            }).Skip(offset).Take(PerPage);
        
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<MessageDTO>> GetMessagesUserTimeline(string username, int page)
    {
        int offset = (page - 1) * PerPage;
        
        var query = (from message in _dbContext.Messages
            orderby message.PubDate descending
            where message.Flagged == 0 && message.User.UserName == username
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.User.UserName,
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                GravatarUrl = message.User.GravatarURL
            }).Skip(offset).Take(PerPage);
        
        var result = await query.ToListAsync();
        return result;
    }
    
    public async Task<List<MessageDTO>> GetMessagesFromUsernameSpecifiedAmount(string username, int amount)
    {

        var query = (from message in _dbContext.Messages
            orderby message.PubDate descending
            where message.Flagged == 0 && message.User.UserName == username
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.User.UserName,
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                GravatarUrl = message.User.GravatarURL
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
                  && (message.User.UserName == username || following.Contains(message.User.Id)) 
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.User.UserName,
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                GravatarUrl = message.User.GravatarURL
            }).Skip(offset).Take(PerPage);
        
        var result = await query.ToListAsync();
        return result;
    }
}