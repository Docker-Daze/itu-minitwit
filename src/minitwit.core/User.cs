namespace minitwit.core;

public class User
{
    public string UserId { get; set; }
    public string Email {get; set;}
    public string? Username { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public string? GravatarUrl { get; set; } = "https://www.gravatar.com/avatar/00000000000000000000000000000000?d=identicon&s=80";
}