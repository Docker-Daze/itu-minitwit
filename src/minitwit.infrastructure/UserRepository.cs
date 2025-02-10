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
    public async Task<User> GetUser(string username)
    {
        var query = from Users in _dbContext.Users
            where Users.UserName == username
            select Users;
        
        var user = await query.FirstOrDefaultAsync();
        return user;
    }

    public Task<string> GetGravatarURL(string email)
    {
        // def gravatar_url(email, size=80): """Return the gravatar image for the given email address."""
        throw new NotImplementedException();
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