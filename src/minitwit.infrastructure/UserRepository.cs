using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using minitwit.core;
using minitwit.web;

namespace minitwit.infrastructure;

public class UserRepository : IUserRepository
{

    private readonly MinitwitDbContext _dbContext;
    private readonly MetricsService _metricsService;

    public UserRepository(MinitwitDbContext dbContext, MetricsService metricsService)
    {
        _dbContext = dbContext;
        _metricsService = metricsService;
    }
    public async Task<User?> GetUser(string userId)
    {
        var query = from Users in _dbContext.Users
                    where Users.Id == userId
                    select Users;

        var user = await query.FirstOrDefaultAsync();
        return user;
    }
    public async Task<User?> GetUserFromUsername(string username)
    {
        var query = from Users in _dbContext.Users
                    where Users.UserName == username
                    select Users;

        var user = await query.FirstOrDefaultAsync();
        return user;
    }

    public async Task<string?> GetUserID(string username)
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
        List<string> ids = await GetUserIDs(whoUsername,whomUsername);
        string whoId = ids[0];
        string whomId = ids[1];
        if (whoId == whomId)
        {
            throw new InvalidOperationException("You cannot follow yourself.");
        }

        bool isFollowing = await IsFollowingUserID(whoId, whomId);
        if (isFollowing)
        {
            throw new InvalidOperationException("You already follow this user");
        }
        
        var follow = new Follower
        {
            WhoId = whoId!,
            WhomId = whomId
        };

        try
        {
            await _dbContext.Followers.AddAsync(follow);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to follow user, possible duplicate entry.", ex);
        }
    }


    public async Task UnfollowUser(string whoUsername, string whomUsername)
    {
        List<string> ids = await GetUserIDs(whoUsername,whomUsername);
        string whoId = ids[0];
        string whomId = ids[1];
        var isFollowing = await IsFollowingUserID(whoId, whomId);
        if (!isFollowing)
        {
            throw new InvalidOperationException("You need to follow the user to unfollow");
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
        var whomId = await GetUserID(whomUsername);
        var whoId = await GetUserID(whoUsername);

        if (string.IsNullOrEmpty(whomId) || string.IsNullOrEmpty(whoId))
        {
            return false; // User not found, cannot be following
        }

        return await _dbContext.Followers
            .AnyAsync(f => f.WhoId == whoId && f.WhomId == whomId);
    }

    public async Task<bool> IsFollowingUserID(string? whoId, string? whomId)
    {
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

        return await _dbContext.Followers
            .Where(f => f.WhoId == userId)
            .Join(_dbContext.Users,
                f => f.WhomId,
                u => u.Id,
                (f, u) => new APIFollowingDTO
                {
                    follows = u.UserName!
                })
            .ToListAsync();
    }

    private async Task<List<string>> GetUserIDs(string whoUsername, string whomUsername)
    {
        List<string> ids = new List<string>();
        
        // Query the Users table once for both usernames.
        var users = await _dbContext.Users
            .Where(u => u.UserName == whoUsername || u.UserName == whomUsername)
            .Select(u => new { u.UserName, u.Id })
            .ToListAsync();
        // Extract the IDs from the result.
        var whoId = users.FirstOrDefault(u => u.UserName == whoUsername)?.Id;
        var whomId = users.FirstOrDefault(u => u.UserName == whomUsername)?.Id;

        // Check if either user was not found.
        if (string.IsNullOrEmpty(whoId) || string.IsNullOrEmpty(whomId))
        {
            throw new InvalidOperationException("One of these users does not exists.");
        }
        ids.Add(whoId);
        ids.Add(whomId);

        return ids;
    }
}