using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MessageRepository : IMessageRepository
{
    private readonly MinitwitDbContext _dbContext;
    
    public MessageRepository(MinitwitDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddMessage(Message message)
    {
        throw new NotImplementedException();
    }

    public async Task<List<MessageDTO>> GetMessages()
    {
        var query = (from message in _dbContext.Messages
            orderby message.Timestamp descending
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.User.Username,
                Timestamp = message.Timestamp.ToString("MM'/'dd'/'yy H':'mm':'ss")
            });
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<MessageDTO>> GetMessagesUserTimeline(string userId)
    {
        throw new NotImplementedException();
    }
}