namespace minitwit.core;

public interface IMessageRepository
{
    public Task AddMessage(string userId, string message);
    public Task<List<MessageDTO>> GetMessages(int page);
    public Task<List<MessageDTO>> GetMessagesUserTimeline(string username, int page);
    public Task<List<MessageDTO>> GetMessagesFromUsernameSpecifiedAmount(string username, int page);
    public Task<List<MessageDTO>> GetMessagesOwnTimeline(string username, int page);
}