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

public class UnFollowerBatchService : BackgroundService
{
    private const int BatchSize = 10;
    private readonly Channel<Follower> _chan;
    private readonly IDbContextFactory<MinitwitDbContext> _factory;

    public UnFollowerBatchService(
        IUnfollowChannel unfollowChannel,
        IDbContextFactory<MinitwitDbContext> factory)
    {
        _chan = unfollowChannel.Channel;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // collect a full batch of unfollow events
            var toUnfollow = new List<Follower>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                var f = await _chan.Reader.ReadAsync(stoppingToken);
                toUnfollow.Add(f);
            }

            await using var ctx = _factory.CreateDbContext();
            ctx.Followers.RemoveRange(toUnfollow);
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }
}