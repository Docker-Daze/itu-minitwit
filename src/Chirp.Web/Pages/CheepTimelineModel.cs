#nullable disable //Remove null Warnings
using System.Security.Claims;
using Chirp.Core;

namespace Chirp.Web.Pages;


/// <summary>
/// Contains shared data and functionality between the public and private timeline.
/// </summary>
public class CheepTimelineModel
{
    public List<CheepDTO> Cheeps { get; set; }
    public Dictionary<string, bool> FollowerMap;
    public Dictionary<string, bool> LikeMap;
    public Dictionary<string, string> AvatarMap;
    
    public CheepTimelineModel()
    {
        FollowerMap = new Dictionary<string, bool>();
        LikeMap = new Dictionary<string, bool>();
        AvatarMap = new Dictionary<string, string>();
    }
    
    public async Task FetchAuthorData(ClaimsPrincipal user, IAuthorRepository authorRepository, ICheepRepository cheepRepository)
    {
        foreach (var cheep in Cheeps)
        {
            if (cheep.Author != user.Identity!.Name)
            {
                if (user.Identity.IsAuthenticated)
                {
                    FollowerMap[cheep.Author] = await authorRepository.IsFollowing(user.Identity.Name!, cheep.Author);
                    LikeMap[cheep.Id] = await cheepRepository.IsLiked(cheep.Id, user.Identity.Name);
                }
            }
            AvatarMap[cheep.Author] = await authorRepository.GetProfilePicture(cheep.Author);
        }
        if (user.Identity!.Name != null)
        {
            AvatarMap[user.Identity.Name] = await authorRepository.GetProfilePicture(user.Identity.Name);
        }
    }
}