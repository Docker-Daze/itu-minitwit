namespace minitwit.core;

public class MessageDTO
{
    public required string Text { get; set; }
    public required string Username { get; set; }
    public required string Timestamp { get; set; }
}