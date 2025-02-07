namespace Chirp.Core;

/// <summary>
/// The ICheepRepository interface defines the methods that the CheepRepository class must implement.
/// </summary>
public interface ICheepRepository
{
    public Task<List<CheepDTO>> GetCheepsByNewest(int currentPage);
    public Task<List<CheepDTO>> GetCheepsFromAuthor(string authorName, int currentPage);
    public Task<List<CheepDTO>> GetCheepsFromFollowersAndOwnCheeps(string userName, int currentPage);
    public Task<Cheep> CreateCheep(string authorName, string message);
    public Task<Author> FindAuthorByName(string name);
    public Task DeleteCheep(string cheepId, string username);
    public Task<List<CheepDTO>> GetAllCheepsFromAuthor(string authorName);
    public Task Like(string cheepId, string userName);
    public Task Unlike(string cheepId, string userName);
    public Task<bool> IsLiked(string cheepId, string userName);
    public Task DeleteLikesFromAuthor(string userName);
}