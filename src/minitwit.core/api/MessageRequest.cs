namespace minitwit.core;

public class MessageRequest
{
    public required string Content { get; set; }
    public int flagged { get; set; }
}