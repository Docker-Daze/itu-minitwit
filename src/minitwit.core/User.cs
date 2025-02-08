namespace minitwit.core;

public class User
{
    public string Username { get; set; }
    public ICollection<Message> Tweets { get; set; } = new List<Message>();
    public string GravatarURL { get; set; }
}