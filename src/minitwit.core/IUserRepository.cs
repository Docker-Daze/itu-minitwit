namespace minitwit.core;

public interface IUserRepository
{
    public Task<User> GetUser(string userId);
    public Task<string> GetGravatarURL(string email, int size);
    public Task FollowUser(string username);
    public Task UnfollowUser(string username);
}