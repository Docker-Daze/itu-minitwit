using minitwit.core;

namespace minitwit.infrastructure;

public class MessageRepository : IMessageRepository
{
    public Task AddMessage(Message message)
    {
        // def add_message(): """Registers a new message for the user."""
        throw new NotImplementedException();
    }

    public Task<ICollection<Message>> GetMessages()
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<Message>> GetMessagesUserTimeline(string userId)
    {
        throw new NotImplementedException();
    }
}