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

    public Task<User> GetUser(string username, int size)
    {
        throw new NotImplementedException();
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

    public Task FollowUser(string username)
    {
        // def follow_user(username): """Adds the current user as follower of the given user."""
        throw new NotImplementedException();
    }

    public Task UnfollowUser(string username)
    {
        // def unfollow_user(username): """Removes the current user as follower of the given user."""
        throw new NotImplementedException();
    }
}