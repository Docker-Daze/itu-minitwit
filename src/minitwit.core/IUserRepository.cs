namespace minitwit.core;

public interface IUserRepository
{
    public Task<User?> GetUser(string userId);
    public Task<User?> GetUserFromUsername(string username);
    public Task<string?> GetUserID(string username);
    public Task<string> GetGravatarURL(string email, int size);
    public Task<Follower> FollowUser(string whoUsername, string whomUsername);
    public Task<Follower> UnfollowUser(string whoUsername, string whomUsername);
    public Task<bool> IsFollowing(string whoUsername, string whomUsername);
    public Task<bool> IsFollowingUserID(string whoId, string whomId);
    public Task<List<APIFollowingDTO>> GetFollowers(string username);
    
    Task AddFollowersBatchAsync(IEnumerable<Follower> follows);
    Task RemoveFollowersBatchAsync(IEnumerable<Follower> follows);

}