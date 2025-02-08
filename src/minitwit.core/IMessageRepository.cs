namespace minitwit.core;

public interface IMessageRepository
{
    public Task AddMessage(Message message);
    public Task<ICollection<Message>> GetMessages();
    public Task<ICollection<Message>> GetMessagesUserTimeline(string userId);
}