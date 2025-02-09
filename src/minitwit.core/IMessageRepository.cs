namespace minitwit.core;

public interface IMessageRepository
{
    public Task AddMessage(string message);
    public Task<List<MessageDTO>> GetMessages(int page);
    public Task<List<MessageDTO>> GetMessagesUserTimeline(string username, int page);
}