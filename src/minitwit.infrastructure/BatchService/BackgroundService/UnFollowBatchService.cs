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
    private readonly IDbContextFactory<MinitwitDbContext> _factory;
    private readonly IUserRepository _userRepository;

    public UnFollowerBatchService(
          IUnfollowChannel unfollowChannel,
          IDbContextFactory<MinitwitDbContext> factory,
          IUserRepository userRepository)
    {
        _chan = unfollowChannel.Channel;
        _factory = factory;
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

            await using var ctx = _factory.CreateDbContext();
            ctx.Followers.RemoveRange(toUnfollow);
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }
}