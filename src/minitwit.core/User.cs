using System.ComponentModel.DataAnnotations;

namespace minitwit.core;

public class User
{
    [Key]
    public string UserId { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; }
    public string Email { get; set; }
    public string pw_hash { get; set; }
    public ICollection<Message> Tweets { get; set; } = new List<Message>();
    public string GravatarURL { get; set; }
}