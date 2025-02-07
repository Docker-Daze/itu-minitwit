namespace Chirp.Core;

/// <summary>
/// The IAuthorRepository interface defines the methods that the AuthorRepository class must implement.
/// </summary>
public interface IAuthorRepository
{
    public Task<Author> CreateAuthor(AuthorDTO authorDto);
    public Task<Author> FindAuthor(string authorName);
    public Task Follow(string userName, string authorName);
    public Task Unfollow(string userName, string authorName);
    public Task<Boolean> IsFollowing(string userName, string authorName);
    public Task<List<string>> GetFollowing(string userName);
    public Task<List<string>> GetFollowers(string userName);
    public Task<int> GetFollowerCount(string userName);
    public Task ChangeProfilePicture(string userName, string? profilePictureLink);
    public Task<string?> GetProfilePicture(string? userName);
}