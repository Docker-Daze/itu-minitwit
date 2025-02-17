using System.Runtime.CompilerServices;
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

    public async Task FollowUser(string whoUsername, string whomUsername)
    {
        var isFollowing = await IsFollowing(whomUsername, whoUsername);
        if (isFollowing)
        {
            throw new InvalidOperationException("You are already following this user");
        }
        
        var whoId = await GetUserID(whoUsername);
        var whomId = await GetUserID(whomUsername);
        
        if (string.IsNullOrEmpty(whomId))
        {
            throw new ArgumentException("The user to be followed does not exist.");
        }
        
        if (whoId == whomId)
        {
            throw new InvalidOperationException("You cannot follow yourself.");
        }

        var follow = new Follower
        {
            WhoId = whoId,
            WhomId = whomId
        };

        await _dbContext.Followers.AddAsync(follow);
        await _dbContext.SaveChangesAsync();
    }


    public async Task UnfollowUser(string whoUsername, string whomUsername)
    {
        var isFollowing = await IsFollowing(whoUsername, whomUsername);
        if (!isFollowing)
        {
            throw new InvalidOperationException("You need to follow the user to unfollow");
        }
        
        var whomId = await GetUserID(whomUsername);
        var whoId = await GetUserID(whoUsername);

        if (string.IsNullOrEmpty(whomId) || string.IsNullOrEmpty(whoId))
        {
            throw new ArgumentException("The user to be unfollowed could not be found");
        }

        var followerEntry = await _dbContext.Followers
            .FirstOrDefaultAsync(f => f.WhoId == whoId && f.WhomId == whomId);

        if (followerEntry == null)
        {
            throw new InvalidOperationException("Not found");
        }

        _dbContext.Followers.Remove(followerEntry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsFollowing(string whoUsername, string whomUsername)
    {
        string whomId = await GetUserID(whomUsername);
        string whoId = await GetUserID(whoUsername);

        if (string.IsNullOrEmpty(whomId) || string.IsNullOrEmpty(whoId))
        {
            return false; // User not found, cannot be following
        }

        return await _dbContext.Followers
            .AnyAsync(f => f.WhoId == whoId && f.WhomId == whomId);
    }

    public async Task<List<APIFollowingDTO>> GetFollowers(string username)
    {
        var userId = await GetUserID(username);

        // Get the IDs of the users that 'username' is following
        var followedUserIds = await _dbContext.Followers
            .Where(f => f.WhoId == userId)
            .Select(f => f.WhomId)
            .ToListAsync();

        var dtos = new List<APIFollowingDTO>();

        foreach (var followedUserId in followedUserIds)
        {
            var user = await GetUser(followedUserId);
            dtos.Add(new APIFollowingDTO
            {
                follows = user.UserName! 
            });
        }

        return dtos;
    }
    


}