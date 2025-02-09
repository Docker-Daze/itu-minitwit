using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MessageRepository : IMessageRepository
{
    private readonly MinitwitDbContext _dbContext;
    private const int pageSize = 10;
    
    public MessageRepository(MinitwitDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddMessage(string message)
    {
        throw new NotImplementedException();
    }

    public async Task<List<MessageDTO>> GetMessages(int page)
    {
        int offset = (page - 1) * pageSize;
        
        var query = (from message in _dbContext.Messages
            orderby message.PubDate descending
            where message.Flagged == 0
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.Author.Username,
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss")
            }).Skip(offset).Take(pageSize);
        
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<MessageDTO>> GetMessagesUserTimeline(string username, int page)
    {
        int offset = (page - 1) * pageSize;
        
        var query = (from message in _dbContext.Messages
            orderby message.PubDate descending
            where message.Flagged == 0 && message.Author.Username == username
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.Author.Username,
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss")
            }).Skip(offset).Take(pageSize);
        
        var result = await query.ToListAsync();
        return result;
    }
}