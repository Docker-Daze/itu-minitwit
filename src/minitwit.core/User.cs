using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;

namespace minitwit.core;

public class User : IdentityUser
{
    public string? pw_hash { get; set; }
    public ICollection<Message> Tweets { get; set; } = new List<Message>();
    public string? GravatarURL { get; set; }
}