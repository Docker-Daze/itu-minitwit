namespace minitwit.core;

public class Message
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string? Text { get; set; }
    public DateTime Timestamp { get; set; }
    public User? User { get; set; }
    
}