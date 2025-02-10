namespace minitwit.core;

public class MessageDTO
{
    public required string Text { get; set; }
    public required string Username { get; set; }
    public required string PubDate { get; set; }
    public required string GravatarUrl { get; set; }
}