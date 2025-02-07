using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chirp.AuthorRepository.Tests;

public static class AuthorRepositoryUtil
{   
    /// <summary>
    /// Get a usermanager, that is used to modify the database.
    /// Used for removing author.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static UserManager<Author> GetUserManager(ChirpDBContext context)
    {
        var userStore = new UserStore<Author>(context);
        var optionsAccessor = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<Author>();
        var userValidators = new List<IUserValidator<Author>> { new UserValidator<Author>() };
        var passwordValidators = new List<IPasswordValidator<Author>> { new PasswordValidator<Author>() };
        var lookupNormalizer = new UpperInvariantLookupNormalizer();
        var errorDescriber = new IdentityErrorDescriber();
        var services = new ServiceCollection();
        var logger = new Logger<UserManager<Author>>(new LoggerFactory());

        var userManager = new UserManager<Author>(
            userStore,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            lookupNormalizer,
            errorDescriber,
            services.BuildServiceProvider(),
            logger
        );
        
        return userManager;
    } 
}