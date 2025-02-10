using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MessageRepository : IMessageRepository
{
    private readonly MinitwitDbContext _dbContext;
    private const int PerPage = 10;
    
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
        int offset = (page - 1) * PerPage;
        
        var query = (from message in _dbContext.Messages
            orderby message.PubDate descending
            where message.Flagged == 0
            select new MessageDTO
            {
                Text = message.Text,
                Username = message.User.UserName,
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss")
            }).Skip(offset).Take(PerPage);
        
        var result = await query.ToListAsync();
        return result;
        throw new NotImplementedException();
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
                PubDate = message.PubDate.ToString("MM'/'dd'/'yy H':'mm':'ss")
            }).Skip(offset).Take(PerPage);
        
        var result = await query.ToListAsync();
        return result;
        throw new NotImplementedException();
    }
}