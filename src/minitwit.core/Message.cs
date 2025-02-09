namespace minitwit.core;

public class Message
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    
    public User Author { get; set; }
    public string? Text { get; set; }
    public DateTime PubDate { get; set; }

    public int Flagged { get; set; } = 0;

}