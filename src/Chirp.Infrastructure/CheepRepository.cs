#nullable disable //remove null warnings
using Chirp.Core;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository
{
    private readonly ChirpDBContext _dbContext;
    private const int pageSize = 32;
    
    public CheepRepository(ChirpDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Gets all cheeps, and puts the newest at the top.
    /// Used for publictimeline. Uses the currentpage number to determine what cheeps to load.
    /// </summary>
    /// <param name="currentPage"></param>
    /// <returns></returns>
    public async Task<List<CheepDTO>> GetCheepsByNewest(int currentPage)
    {
        int offset = (currentPage - 1) * pageSize;

        var query = (from cheep in _dbContext.Cheeps
            orderby cheep.TimeStamp descending
            select new CheepDTO
            {
                Id = cheep.CheepId,
                Author = cheep.Author.UserName,
                Text = cheep.Text,
                TimeStamp = cheep.TimeStamp.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                LikeCount = cheep.Likes.Count.ToString(),
            }).Skip(offset).Take(pageSize);
        
        var result = await query.ToListAsync();
        return result;
    }
    
    
    /// <summary>
    /// Used for loading all cheeps from an author name.
    /// Used for about me or downloading a users data.
    /// </summary>
    /// <param name="authorName"></param>
    /// <returns></returns>
    public async Task<List<CheepDTO>> GetAllCheepsFromAuthor(string authorName)
    {
        var query = (from cheep in _dbContext.Cheeps
            orderby cheep.TimeStamp descending
            where cheep.Author.UserName == authorName
            select new CheepDTO
            {
                Id = cheep.CheepId,
                Author = cheep.Author.UserName,
                Text = cheep.Text,
                TimeStamp = cheep.TimeStamp.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                LikeCount = cheep.Likes.Count.ToString(),
            });
        
        var result = await query.ToListAsync();
        return result;
    }
    
    /// <summary>
    /// Useed for loading another users private timeline, where a user needs another users cheeps.
    /// It uses the currentpage number to determine what cheeps to load.
    /// </summary>
    /// <param name="authorName"></param>
    /// <param name="currentPage"></param>
    /// <returns></returns>
    public async Task<List<CheepDTO>> GetCheepsFromAuthor(string authorName, int currentPage)
    {
        int offset = (currentPage - 1) * pageSize;

        var query = (from cheep in _dbContext.Cheeps
            orderby cheep.TimeStamp descending
            where cheep.Author.UserName == authorName
            select new CheepDTO
            {
                Id = cheep.CheepId,
                Author = cheep.Author.UserName,
                Text = cheep.Text,
                TimeStamp = cheep.TimeStamp.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                LikeCount = cheep.Likes.Count.ToString(),
            }).Skip(offset).Take(pageSize);
        var result = await query.ToListAsync();
        
        return result;
    }
    /// <summary>
    /// Used for loading the user timeline, where a user neeeds to see their own cheeps.
    /// It uses the currentpage number to determine what cheeps to load.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="currentPage"></param>
    /// <returns></returns>
    public async Task<List<CheepDTO>> GetCheepsFromFollowersAndOwnCheeps(string userName, int currentPage)
    {
        int offset = (currentPage - 1) * pageSize;
        var user = await _dbContext.Authors
            .Include(a => a.Following)
            .FirstOrDefaultAsync(a => a.UserName == userName);
        
        if (user == null)
        {
            return new List<CheepDTO>();
        }
        
        var query = (from cheep in _dbContext.Cheeps
            orderby cheep.TimeStamp descending
            where user.Following.Contains(cheep.Author) || cheep.Author.UserName == userName
            select new CheepDTO
            {
                Id = cheep.CheepId,
                Author = cheep.Author.UserName,
                Text = cheep.Text,
                TimeStamp = cheep.TimeStamp.ToString("MM'/'dd'/'yy H':'mm':'ss"),
                LikeCount = cheep.Likes.Count.ToString(),
            }).Skip(offset).Take(pageSize);
        var result = await query.ToListAsync();

        return result;
    }
    
    /// <summary>
    /// Used for creating cheeps. The cheeps are checked for length
    /// and whitespace to see if the cheep is valid.
    /// If it is valid it creates a new cheep and inserts it into the database
    /// </summary>
    /// <param name="authorName"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Cheep>  CreateCheep(string authorName, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Cheep text cannot be empty.");
        }
        if (text.Length > 160)
        {
            throw new ArgumentException("Cheep text cannot exceed 160 characters.");
        }
        
        Author author = await FindAuthorByName(authorName);
        Cheep newCheep = new()
        {
            Text = text,
            TimeStamp = DateTime.Now, 
            Author = author,
        };
        
        await _dbContext.Cheeps.AddAsync(newCheep); // does not write to the database!

        await _dbContext.SaveChangesAsync(); // persist the changes in the database
        return newCheep;
    }
    
    /// <summary>
    /// Deletes a specified cheep from the database.
    /// Cheeps can only be deleted by the author of the cheep.
    /// </summary>
    /// <param name="cheepId">The cheep to be deleted</param>
    /// <param name="userName">The username of the user initiating the deletion</param>
    public async Task DeleteCheep(string cheepId, string userName)
    {
        var cheep = await _dbContext.Cheeps
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.CheepId == cheepId);
        
        if (cheep != null && cheep.Author.UserName == userName)
        {
            await DeleteLikesFromCheep(cheep);
            _dbContext.Cheeps.Remove(cheep);
        }
        
        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Creates a new like relation in the database between the specified cheep and user.
    /// </summary>
    /// <param name="cheepId"></param>
    /// <param name="userName"></param>
    public async Task Like(string cheepId, string userName)
    {
        var cheep = await _dbContext.Cheeps
            .Where(c => c.CheepId == cheepId)
            .Select(c => c.Author)
            .FirstOrDefaultAsync();
        
        if (cheep == null || cheep.UserName == userName)
        {
            return;
        }

        var author = await FindAuthorByName(userName);
        var likeExists = false;
        if (author != null)
        {
            likeExists = await _dbContext.Likes
                .AnyAsync(l => l.CheepId == cheepId && l.AuthorId == author.Id);
        }
        
        if (!likeExists)
        {
            var like = new Like
            {
                AuthorId = author.Id,
                CheepId = cheepId
            };
           await  _dbContext.Likes.AddAsync(like);
        }
        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Finds and removes a like relation from the database between a specified cheep and user.
    /// </summary>
    /// <param name="cheepId"></param>
    /// <param name="userName"></param>
    public async Task Unlike(string cheepId, string userName)
    {
        var author = await FindAuthorByName(userName);
        if (author != null)
        {
            var like = await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.CheepId == cheepId && l.AuthorId == author.Id);
            _dbContext.Likes.Remove(like);
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Determines whether the specified cheep is liked by the specified user.
    /// </summary>
    /// <param name="cheepId"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<bool> IsLiked(string cheepId, string userName)
    {
        var author = await FindAuthorByName(userName);
        var like = await _dbContext.Likes.FirstOrDefaultAsync(l => l.CheepId == cheepId && l.AuthorId == author.Id);

        return like != null;
    }
    
    public async Task<Author> FindAuthorByName(string name)
    {
        var query = from author in _dbContext.Authors
            where (author.UserName == name)
            select author;

        var foundAuthor = await query.FirstOrDefaultAsync();

        return foundAuthor;
    }

    /// <summary>
    /// Deletes all likes associated with a user from the database.
    /// </summary>
    /// <param name="userName"></param>
    public async Task DeleteLikesFromAuthor(string userName)
    {
        var author = await FindAuthorByName(userName);
        var delete = from like in _dbContext.Likes
                where like.AuthorId == author.Id
                select like;

        if (delete.Any())
        {
            _dbContext.Likes.RemoveRange(delete);
        }

        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes all likes associated with a specified cheep.
    /// Used for likes before deleting a cheep.
    /// </summary>
    /// <param name="cheep"></param>
    private async Task DeleteLikesFromCheep(Cheep cheep)
    {
        var likesToDelete = _dbContext.Likes.Where(like => like.CheepId == cheep.CheepId);

        _dbContext.Likes.RemoveRange(likesToDelete);

        await _dbContext.SaveChangesAsync();
    }
}