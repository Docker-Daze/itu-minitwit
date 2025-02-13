// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace minitwit.web.Pages;

public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IUserRepository _userRepository;

    public RegisterModel(
        UserManager<User> userManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _userRepository = userRepository;
    }
    
    [BindProperty]
    public string Username { get; set; }
    [BindProperty]
    public string Email { get; set; }
    [BindProperty]
    public string Password { get; set; }
    [BindProperty]
    public string Password2 { get; set; }
    public string ReturnUrl { get; set; }
    

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (string.IsNullOrEmpty(Username))
        {
            ModelState.AddModelError(string.Empty, "You have to enter a username");
            return Page();
        }
        else if (string.IsNullOrEmpty(Email) || !Email.Contains("@"))
        {
            ModelState.AddModelError(string.Empty, "You have to enter a valid email address");
            return Page();
        }
        else if (string.IsNullOrEmpty(Password))
        {
            ModelState.AddModelError(string.Empty,"You have to enter a password");
            return Page();
        }
        else if (Password != Password2)
        {
            ModelState.AddModelError(string.Empty,"The two passwords do not match");
            return Page();
        }
        else if (await _userManager.FindByNameAsync(Username) is not null)
        {
            TempData["FlashMessage"] ="The username is already taken";
            return Page();
        }
        
        if (ModelState.IsValid)
        {
            var user = CreateUser();

            user.UserName = Username;
                
            await _userStore.SetUserNameAsync(user, Username, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Password);
                
            if (result.Succeeded)
            {
                TempData["FlashMessage"] ="You were successfully registered and can login now";

                user.GravatarURL = await _userRepository.GetGravatarURL(Email, 80);
                
                return LocalRedirect("/login");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private User CreateUser()
    {
        try
        {
            return Activator.CreateInstance<User>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                                                $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<User>)_userStore;
    }
}