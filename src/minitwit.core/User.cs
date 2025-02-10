using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;

namespace minitwit.core;

public class User : IdentityUser
{
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public string GravatarURL { get; set; } = "https://www.gravatar.com/avatar/00000000000000000000000000000000?d=identicon&s=80";
}