using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace minitwit.core;

public class User : IdentityUser
{
    public string Username { get; set; }
    public ICollection<Message> Tweets { get; set; } = new List<Message>();
    public string GravatarURL { get; set; }
}