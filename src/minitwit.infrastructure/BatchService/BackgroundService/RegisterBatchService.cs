using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using minitwit.core;
using Serilog;

namespace minitwit.infrastructure
{
    public class RegisterBatchService : BackgroundService
    {
        private const int BatchSize = 2;
        private readonly IRegisterChannel _registerChannel;
        private readonly IServiceScopeFactory _scopeFactory;

        public RegisterBatchService(IRegisterChannel registerChannel, IServiceScopeFactory scopeFactory)
        {
            _registerChannel = registerChannel;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var userStore = scope.ServiceProvider.GetRequiredService<IUserStore<User>>();
                var emailStore = (IUserEmailStore<User>)userStore;
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var request = await _registerChannel.Channel.Reader.ReadAsync(stoppingToken);
                try
                {
                    var user = Activator.CreateInstance<User>();

                    user.UserName = request.username;

                    var existingUser = await userManager.FindByEmailAsync(request.email);
                    if (existingUser != null || user.UserName == null)
                    {
                        throw new Exception("Email address already exists.");
                    }

                    await userStore.SetUserNameAsync(user, request.username, CancellationToken.None);
                    await emailStore.SetEmailAsync(user, request.email, CancellationToken.None);
                    var result = await userManager.CreateAsync(user, request.pwd);
                    if (result.Succeeded)
                    {
                        user.GravatarURL = await userRepository.GetGravatarURL(request.email, 80);
                        await userManager.UpdateAsync(user);
                        Log.Warning("Successfully added {username}", request.username);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error reading register request");
                }
            }
        }
    }
}
