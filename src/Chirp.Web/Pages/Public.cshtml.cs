#nullable disable //Remove null warnings
using Chirp.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

/// <summary>
/// The public timeline. Responsible for hosting "the main page" of the application.
/// </summary>
public class PublicModel : PageModel
{
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;
    public int _currentPage;
    public bool _nextPageHasCheeps;
    public CheepTimelineModel CheepTimelineModel { get; private set; } = new();
    [BindProperty]
    public CheepInputModel CheepInput { get; set; }
    
    public PublicModel(UserManager<Author> userManager, ICheepRepository cheepRepository, IAuthorRepository authorRepository)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
    }

    public async Task<ActionResult> OnGet([FromQuery] int? page)
    {
        _currentPage = page ?? 1;
        CheepTimelineModel.Cheeps = await GetCheeps(_currentPage);
        await CheepTimelineModel.FetchAuthorData(User, _authorRepository, _cheepRepository);
        _nextPageHasCheeps = await NextPageHasCheeps(_currentPage);
        
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
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
            CheepTimelineModel.Cheeps = await GetCheeps(_currentPage);
            await CheepTimelineModel.FetchAuthorData(User, _authorRepository, _cheepRepository);
            return Page();
        }

        await _cheepRepository.CreateCheep(User.Identity.Name, message);
        return Redirect("/");
    }

    public async Task<IActionResult> OnPostFollow(string authorName, int? page)
    {
        if (User.Identity.Name == authorName)
        {
            ModelState.AddModelError(string.Empty, "You cannot follow yourself.");
            return Redirect($"/?page={page}");
        }
        
        await _authorRepository.Follow(User.Identity.Name, authorName);
        CheepTimelineModel.FollowerMap[authorName] = true;
            
        return Redirect($"/?page={page}");
    }
    
    public async Task<IActionResult> OnPostUnfollow(string authorName, int? page)
    {
        await _authorRepository.Unfollow(User.Identity.Name, authorName);
        CheepTimelineModel.FollowerMap[authorName] = false;

        return Redirect($"/?page={page}");
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
        
        return RedirectToPage("Public");
    }
    
    public async Task<IActionResult> OnPostToggleLike(string cheepId, int? page)
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

        return Redirect($"/?page={page}");
    }

    private async Task<List<CheepDTO>> GetCheeps(int page)
    {
         return await _cheepRepository.GetCheepsByNewest(page);
    }

    private async Task<bool> NextPageHasCheeps(int page)
    {
        var list = await _cheepRepository.GetCheepsByNewest(page + 1);
        return list.Any();
    }
}
