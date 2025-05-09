namespace minitwit.infrastructure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using minitwit.core;
using minitwit.infrastructure;
using Serilog;

public class UnFollowerBatchService : BackgroundService
{
    private const int BatchSize = 10;
    private readonly Channel<string[]> _chan;
    private readonly IUserRepository _userRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public UnFollowerBatchService(
          IUnfollowChannel unfollowChannel,
          IServiceScopeFactory scopeFactory,
          IUserRepository userRepository)
    {
        _chan = unfollowChannel.Channel;
        _scopeFactory = scopeFactory;
        _userRepository = userRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // collect a full batch of unfollow events
            var toUnfollow = new List<Follower>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                var theRequest = await _chan.Reader.ReadAsync(stoppingToken);
                try
                {
                    Follower request = await _userRepository.UnfollowUser(theRequest[0], theRequest[1]);
                    toUnfollow.Add(request);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "User {User} tried to unfollow {Target} but something went wrong", theRequest[0], theRequest[1]);
                    continue;
                }
            }

            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
            await ctx.Followers.AddRangeAsync(toUnfollow, stoppingToken);
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }
}