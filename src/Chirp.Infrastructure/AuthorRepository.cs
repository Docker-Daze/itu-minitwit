#nullable disable //remove null warnings
using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

/// <summary>
/// The AuthorRepository class is used to do CRUD operations on the database.
/// </summary>
public class AuthorRepository : IAuthorRepository
{
    private readonly ChirpDBContext _dbContext;

    /// <summary>
    /// Initializes a new instance of AuthorRepository.
    /// </summary>
    /// <param name="dbContext"></param>
    public AuthorRepository(ChirpDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Creates an author with an AuthorDTO.
    /// </summary>
    /// <param name="authorDto"></param>
    /// <returns></returns>
    public async Task<Author> CreateAuthor(AuthorDTO authorDto)
    {
        Author newAuthor = new() { UserName = authorDto.Name, Email = authorDto.Email, ProfilePicture = "https://cdn.pixabay.com/photo/2024/01/29/09/06/ai-generated-8539307_1280.png"};
        var queryResult = await _dbContext.Authors.AddAsync(newAuthor); // does not write to the database!
        
        await _dbContext.SaveChangesAsync(); // persist the changes in the database
        return queryResult.Entity;
    }
    
    /// <summary>
    /// Finds the author from the database with the given name.
    /// </summary>
    /// <param name="authorName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">An author was not found.</exception>
    public async Task<Author> FindAuthor(string authorName)
    {
        var query = from author in _dbContext.Authors
                .Include(a => a.Following)
                .Include(a => a.Followers)
            where (author.UserName == authorName)
            select author;

        var result = await query.Distinct().FirstOrDefaultAsync();


        if (result == null)
        {
            throw new InvalidOperationException($"Author with name '{authorName}' was not found.");
        }

        return result;
    }

    /// <summary>
    /// Adds the specified author to the follower/following relationship between the two users.
    /// </summary>
    /// <param name="userName">The username of the user initiating the follow action</param>
    /// <param name="authorName">The username of the user to be followed.</param>
    public async Task Follow(string userName, string authorName)
    {
        if (userName == authorName)
        {
            return;
        }
        
        var user = await FindAuthor(userName);
        var author = await FindAuthor(authorName);
        
        if (!user.Following.Contains(author))
        {
            user.Following.Add(author);
        }
        
        if (!author.Followers.Contains(user))
        {
            author.Followers.Add(user);
        }
        
        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes the specified author from the follower/following relationship between the two users.
    /// </summary>
    /// <param name="userName">The username of the user initiating the unfollow action.</param>
    /// <param name="authorName">The username of the user to be unfollowed.</param>
    public async Task Unfollow(string userName, string authorName)
    {
        if (userName == authorName)
        {
            return;
        }
        
        var user = await FindAuthor(userName);
        var author = await FindAuthor(authorName);
        
        if (user.Following.Contains(author))
        {
            user.Following.Remove(author);
        }

        if (author.Followers.Contains(user))
        {
            author.Followers.Remove(user);
        }
        
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Determines whether the specified user is following the specified author.
    /// </summary>
    /// <param name="userName">The username of the user initiating the check.</param>
    /// <param name="authorName">The username of the author being checked.</param>
    /// <returns></returns>
    public async Task<bool> IsFollowing(string userName, string authorName)
    {
        if (userName == authorName)
        {
            return false;
        }
        
        var author = await FindAuthor(authorName);
        var user = await FindAuthor(userName);
        
        return user.Following.Contains(author);
    }

    /// <summary>
    /// Retrieves a list of usernames that the specified user is following.
    /// </summary>
    /// <param name="userName">The username of the user whose following list is to be retrieved.</param>
    /// <returns></returns>
    public async Task<List<string>> GetFollowing(string userName)
    {
        var query = from author in _dbContext.Authors
            where author.UserName == userName
            from following in author.Following
            select following.UserName;
        
        var result = await query.ToListAsync();
        return result;
    }
    
    /// <summary>
    /// Retrieves a list of usernames who are following the specified user.
    /// </summary>
    /// <param name="userName">The username of the user whose followers are to be retrieved.</param>
    /// <returns></returns>
    public async Task<List<string>> GetFollowers(string userName)
    {
        var query = from author in _dbContext.Authors
            where author.UserName == userName
            from follower in author.Followers
            select follower.UserName;
        
        var result = await query.ToListAsync();
        return result;
    }
    
    /// <summary>
    /// Retrieves the total number of followers for the specified user.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<int> GetFollowerCount(string userName)
    {
        var query = from author in _dbContext.Authors
            where author.UserName == userName
            from follower in author.Followers
            select follower.UserName;
        
        var result = await query.ToListAsync();
        return result.Count;
    }

    /// <summary>
    /// Changes the profile picture of the specified user.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="profilePictureLink"></param>
    public async Task ChangeProfilePicture(string userName, string? profilePictureLink)
    {
        if (profilePictureLink == null)
        {
            return;
        }
        var author = await FindAuthor(userName);
        if (author == null) return;
        
        author.ProfilePicture = profilePictureLink;
        
        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Retrieves the profile picture of the specified user.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<string?> GetProfilePicture(string userName)
    {
        if (userName == null)
        {
            return null;
        }
        
        var query = from author in _dbContext.Authors
            where author.UserName == userName
            select author.ProfilePicture;
        var result = await query.Distinct().FirstOrDefaultAsync();
        
        return result;
    }
}