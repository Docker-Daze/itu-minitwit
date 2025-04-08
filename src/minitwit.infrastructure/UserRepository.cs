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
    public async Task<User> GetUser(string userId)
    {
        var query = from Users in _dbContext.Users
                    where Users.Id == userId
                    select Users;

        var user = await query.FirstOrDefaultAsync();

        return user!;
    }
    public async Task<User> GetUserFromUsername(string username)
    {
        var query = from Users in _dbContext.Users
                    where Users.UserName == username
                    select Users;

        var user = await query.FirstOrDefaultAsync();

        return user!;
    }

    public async Task<string?> GetUserID(string username)
    {
        var query = from Users in _dbContext.Users
                    where Users.UserName == username
                    select Users.Id;

        var id = await query.FirstOrDefaultAsync();
        if (id != null) return id;
        return null;
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

    public async Task FollowUser(string whoUsername, string whomUsername, string? whoId = null)
    {
        whoId ??= await GetUserID(whoUsername);
        var whomId = await GetUserID(whomUsername);
        var isFollowing = await IsFollowingUserID(whoId, whomId);
        if (isFollowing)
        {
            throw new InvalidOperationException("You are already following this user");
        }

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


    public async Task UnfollowUser(string whoUsername, string whomUsername, string? whoId)
    {
        whoId ??= await GetUserID(whoUsername);
        var whomId = await GetUserID(whomUsername);

        var isFollowing = await IsFollowingUserID(whoId, whomId);
        if (!isFollowing)
        {
            _metricsService.IncrementUnfollowNeedToFollowCounter();
            throw new InvalidOperationException("You need to follow the user to unfollow");
        }

        if (string.IsNullOrEmpty(whomId) || string.IsNullOrEmpty(whoId))
        {
            _metricsService.IncrementUnfollowfollowerEntryNullCounter();
            throw new ArgumentException("The user to be unfollowed could not be found");
        }

        var followerEntry = await _dbContext.Followers
            .FirstOrDefaultAsync(f => f.WhoId == whoId && f.WhomId == whomId);

        if (followerEntry == null)
        {
            _metricsService.IncrementUnfollowNoWhoOrWhomCounter();
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