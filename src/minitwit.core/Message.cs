using System.ComponentModel.DataAnnotations;

namespace minitwit.core;

public class Message
{
    [Key]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string? Text { get; set; }
    public DateTime Timestamp { get; set; }
    public User? User { get; set; }
    public string UserId { get; set; }
    public int flagged { get; set; }
    
}