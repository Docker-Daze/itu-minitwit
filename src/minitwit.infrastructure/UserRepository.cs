using minitwit.core;

namespace minitwit.infrastructure;

public class UserRepository : IUserRepository
{
    public Task<string> GetUserId(string username)
    {
        // get_user_id(username): """Convenience method to look up the id for a username."""
        throw new NotImplementedException();
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