using System.ComponentModel.DataAnnotations;

namespace minitwit.core;

public class Message
{
    [Key]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public User? User { get; set; }
    public string? Text { get; set; }
    public DateTime PubDate { get; set; }
    public int Flagged { get; set; } = 0;
}