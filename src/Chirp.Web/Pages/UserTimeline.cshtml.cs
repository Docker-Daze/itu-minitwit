#nullable disable //Remove null warnings
using Chirp.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

/// <summary>
/// The private timeline of each user. Where a user sees their own and followed users cheeps.
/// </summary>
public class UserTimelineModel : PageModel
{
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;
    public int _currentPage;
    public bool _nextPageHasCheeps;
    public int followerCount { get; set; }
    public CheepTimelineModel CheepTimelineModel { get; private set; } = new();
    [BindProperty]
    public CheepInputModel CheepInput { get; set; }
    
    public UserTimelineModel(UserManager<Author> userManager, ICheepRepository cheepRepository, IAuthorRepository authorRepository)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
    }

    public async Task<ActionResult> OnGet(string author, [FromQuery] int? page)
    {
        _currentPage = page ?? 1;
        ViewData["CurrentPage"] = _currentPage;
        
        CheepTimelineModel.Cheeps = await GetCheeps(author, _currentPage);
        await CheepTimelineModel.FetchAuthorData(User, _authorRepository, _cheepRepository);
        followerCount = await _authorRepository.GetFollowerCount(author);
        _nextPageHasCheeps = await NextPageHasCheeps(author, _currentPage);
        
        return Page();
        
    }
    
    public async Task<IActionResult> OnPost(string author)
    {
        var message = CheepInput.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError("Message", "Message cannot be empty.");
        }
        else if (message.Length > 160)
        {
            ModelState.AddModelError("Message", "Message cannot be more 160 characters.");
        }
        if (!ModelState.IsValid)
        {
            CheepTimelineModel.Cheeps = await GetCheeps(author, _currentPage); 
            await CheepTimelineModel.FetchAuthorData(User, _authorRepository, _cheepRepository);
            return Page();
        }

        await _cheepRepository.CreateCheep(User.Identity.Name, message);
        return RedirectToPage("UserTimeline", new { page = _currentPage });
    }
    
    public async Task<IActionResult> OnPostDelete(string cheepId, string author)
    {
        if (author != User.Identity.Name)
        {
            ModelState.AddModelError(cheepId, "Cannot delete cheep.");
            return RedirectToPage("Public");
        }
        
        try
        {
            await _cheepRepository.DeleteCheep(cheepId, User.Identity.Name);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine("Unable to delete cheep. Error: " + e.Message);
        }
        return RedirectToPage("UserTimeline");
    }
    
    public async Task<IActionResult> OnPostFollow(string author, string authorName, int? page)
    {
        if (User.Identity.Name == authorName)
        {
            ModelState.AddModelError(string.Empty, "You cannot follow yourself.");
            return Redirect($"/?page={page}");
        }
        
        await _authorRepository.Follow(User.Identity.Name, authorName);
        
        CheepTimelineModel.FollowerMap[authorName] = true;
        return LocalRedirect($"/{author}?page={page}");
    }
    
    public async Task<IActionResult> OnPostUnfollow(string author, string authorName, int? page)
    {
        await _authorRepository.Unfollow(User.Identity.Name, authorName);
        
        CheepTimelineModel.FollowerMap[authorName] = false;
        return LocalRedirect($"/{author}?page={page}");
    }

    public async Task<bool> IsFollowing(string userName, string authorName)
    {
        return await _authorRepository.IsFollowing(userName, authorName);
    }
    
    public async Task<IActionResult> OnPostToggleLike(string author, string cheepId, int? page)
    {
        var isLiked = await _cheepRepository.IsLiked(cheepId, User.Identity.Name);
        if (isLiked)
        {
            await _cheepRepository.Unlike(cheepId, User.Identity.Name);
            CheepTimelineModel.LikeMap[cheepId] = false;
        }
        else
        {
            await _cheepRepository.Like(cheepId, User.Identity.Name);
            CheepTimelineModel.LikeMap[cheepId] = true;
        }

        return Redirect($"/{author}?page={page}");
    }
    
    private async Task<List<CheepDTO>> GetCheeps(string author, int page)
    {
        
        if (author == User.Identity.Name)
        {
            return await _cheepRepository.GetCheepsFromFollowersAndOwnCheeps(author, page);
        }
        return await _cheepRepository.GetCheepsFromAuthor(author, page);
    }
    
    public async Task<bool> NextPageHasCheeps(string author, int page)
    {   
        if (author == User.Identity.Name)
        {
            var list = await _cheepRepository.GetCheepsFromFollowersAndOwnCheeps(author, page+1);
            return list.Any();
        }
        else
        {
            var list = await _cheepRepository.GetCheepsFromAuthor(author, page+1);
            return list.Any();
        }
    }
}
