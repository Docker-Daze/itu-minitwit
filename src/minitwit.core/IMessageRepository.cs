namespace minitwit.core;

public interface IMessageRepository
{
    public Task AddMessage(Message message);
    public Task<List<MessageDTO>> GetMessages();
    public Task<List<MessageDTO>> GetMessagesUserTimeline(string userId);
}