namespace minitwit.core;

public interface IMessageRepository
{
    public Task AddMessage(User user, string message, int flagged = 0);
    public Task<List<MessageDTO>> GetMessages(int page);
    public Task<List<APIMessageDTO>> GetMessagesSpecifiedAmount(int amount);
    public Task<List<MessageDTO>> GetMessagesUserTimeline(string username, int page);
    public Task<List<APIMessageDTO>> GetMessagesFromUsernameSpecifiedAmount(string username, int amount);
    public Task<List<MessageDTO>> GetMessagesOwnTimeline(string username, int page);
}