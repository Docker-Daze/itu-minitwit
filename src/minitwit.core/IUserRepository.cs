namespace minitwit.core;

public interface IUserRepository
{
    public Task<User?> GetUser(string userId);
    public Task<User?> GetUserFromUsername(string username);
    public Task<string?> GetUserID(string username);
    public Task<string> GetGravatarURL(string email, int size);
    public Task FollowUser(string who, string whom);
    public Task UnfollowUser(string who, string whom);
    public Task<bool> IsFollowing(string who, string whom);
    public Task<bool> IsFollowingUserID(string whomId, string whoId);
    public Task<List<APIFollowingDTO>> GetFollowers(string username);

}