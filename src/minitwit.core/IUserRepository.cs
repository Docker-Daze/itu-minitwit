namespace minitwit.core;

public interface IUserRepository
{
    public Task<string> GetUserId(string username);
    public Task<string> GetGravatarURL(string email);
    public Task FollowUser(string username);
    public Task UnfollowUser(string username);
}