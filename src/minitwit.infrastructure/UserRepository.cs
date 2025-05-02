using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using minitwit.core;
using minitwit.web;

namespace minitwit.infrastructure;

public class UserRepository : IUserRepository
{

    private readonly IDbContextFactory<MinitwitDbContext> _factory;

    public UserRepository(MetricsService metricsService, IDbContextFactory<MinitwitDbContext> factory)
    {
        _factory = factory;
    }
    public async Task<User?> GetUser(string userId)
    {
        await using var ctx = _factory.CreateDbContext();
        var query = from Users in ctx.Users
                    where Users.Id == userId
                    select Users;

        var user = await query.FirstOrDefaultAsync();
        return user;
    }
    public async Task<User?> GetUserFromUsername(string username)
    {
        await using var ctx = _factory.CreateDbContext();
        var query = from Users in ctx.Users
                    where Users.UserName == username
                    select Users;

        var user = await query.FirstOrDefaultAsync();
        return user;
    }

    public async Task<string?> GetUserID(string username)
    {
        await using var ctx = _factory.CreateDbContext();
        var query = from Users in ctx.Users
                    where Users.UserName == username
                    select Users.Id;

        var id = await query.FirstOrDefaultAsync();
        return id;
    }

    public async Task<string> GetGravatarURL(string email, int size = 80)
    {
        string normalizedEmail = email.Trim().ToLower();

        // Compute the MD5 hash of the email
        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(normalizedEmail));
        string hash = Convert.ToHexStringLower(hashBytes);

        // Return the gravatar URL asynchronously
        return await Task.FromResult($"http://www.gravatar.com/avatar/{hash}?d=identicon&s={size}");
    }

    public async Task<Follower> FollowUser(string whoUsername, string whomUsername)
    {
        List<string> ids = await GetUserIDs(whoUsername, whomUsername);
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

        return follow;
    }


    public async Task<Follower> UnfollowUser(string whoUsername, string whomUsername)
    {
        List<string> ids = await GetUserIDs(whoUsername, whomUsername);
        string whoId = ids[0];
        string whomId = ids[1];
        var isFollowing = await IsFollowingUserID(whoId, whomId);
        if (!isFollowing)
        {
            throw new InvalidOperationException("You need to follow the user to unfollow");
        }

        await using var ctx = _factory.CreateDbContext();
        var followerEntry = await ctx.Followers
            .FirstOrDefaultAsync(f => f.WhoId == whoId && f.WhomId == whomId);

        if (followerEntry == null)
        {
            throw new InvalidOperationException("Not found");
        }

        return followerEntry;
    }

    public async Task<bool> IsFollowing(string whoUsername, string whomUsername)
    {
        await using var ctx = _factory.CreateDbContext();
        var whomId = await GetUserID(whomUsername);
        var whoId = await GetUserID(whoUsername);

        if (string.IsNullOrEmpty(whomId) || string.IsNullOrEmpty(whoId))
        {
            return false; // User not found, cannot be following
        }

        return await ctx.Followers
            .AnyAsync(f => f.WhoId == whoId && f.WhomId == whomId);
    }

    public async Task<bool> IsFollowingUserID(string? whoId, string? whomId)
    {
        await using var ctx = _factory.CreateDbContext();
        if (string.IsNullOrEmpty(whomId) || string.IsNullOrEmpty(whoId))
        {
            return false; // User not found, cannot be following
        }

        return await ctx.Followers
            .AnyAsync(f => f.WhoId == whoId && f.WhomId == whomId);
    }

    public async Task<List<APIFollowingDTO>> GetFollowers(string username)
    {
        await using var ctx = _factory.CreateDbContext();
        var userId = await GetUserID(username);

        return await ctx.Followers
            .Where(f => f.WhoId == userId)
            .Join(ctx.Users,
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
        await using var ctx = _factory.CreateDbContext();
        List<string> ids = new List<string>();

        // Query the Users table once for both usernames.
        var users = await ctx.Users
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
        ids.Add(whoId!);
        ids.Add(whomId);

        return ids;
    }
}