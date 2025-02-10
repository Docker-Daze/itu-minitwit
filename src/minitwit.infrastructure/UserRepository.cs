using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class UserRepository : IUserRepository
{
    
    private readonly MinitwitDbContext _dbContext;
    
    public UserRepository(MinitwitDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<User> GetUser(string userId)
    {
        var query = from Users in _dbContext.Users
            where Users.Id == userId
            select Users;
        
        var user = await query.FirstOrDefaultAsync();
        return user;
    }

    public async Task<string> GetUserID(string username)
    {
        var query = from Users in _dbContext.Users
            where Users.UserName == username
            select Users.Id;

        var id = await query.FirstOrDefaultAsync();
        return id;
    }
    
    
    public Task<string> GetGravatarURL(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetGravatarURL(string email, int size = 80)
    {
        string normalizedEmail = email.Trim().ToLower();

        // Compute the MD5 hash of the email
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(normalizedEmail));
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Return the gravatar URL asynchronously
            return await Task.FromResult($"http://www.gravatar.com/avatar/{hash}?d=identicon&s={size}");
        }
    }

    public async Task<bool> FollowUser(string whoId, string whomUsername)
{
    string whomId = await GetUserID(whomUsername);

    if (string.IsNullOrEmpty(whomId))
    {
        return false; // User not found, follow operation failed
    }

    var newFollowing = new Follower
    {
        WhoId = whoId,
        WhomId = whomId
    };

    await _dbContext.Followers.AddAsync(newFollowing);
    await _dbContext.SaveChangesAsync();

    return true; // Follow operation successful
}


    public async Task<bool> UnfollowUser(string whoId, string whomUsername)
    {
        string whomId = await GetUserID(whomUsername);

        if (string.IsNullOrEmpty(whomId))
        {
            return false; // User not found, unfollow operation failed
        }

        var followerEntry = await _dbContext.Followers
            .FirstOrDefaultAsync(f => f.WhoId == whoId && f.WhomId == whomId);

        if (followerEntry == null)
        {
            return false; // No follow entry found
        }

        _dbContext.Followers.Remove(followerEntry);
        await _dbContext.SaveChangesAsync();

        return true; // Successfully unfollowed
    }


}