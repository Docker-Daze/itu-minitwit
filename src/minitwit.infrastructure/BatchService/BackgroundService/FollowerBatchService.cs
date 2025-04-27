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


public class FollowerBatchService : BackgroundService
{
    private const int BatchSize = 10;
    private readonly Channel<Follower> _chan;
    private readonly IDbContextFactory<MinitwitDbContext> _factory;

    public FollowerBatchService(
        IFollowChannel followChannel,
        IDbContextFactory<MinitwitDbContext> factory)
    {
        _chan    = followChannel.Channel;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // collect a full batch of follow events
            var toFollow = new List<Follower>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                var f = await _chan.Reader.ReadAsync(stoppingToken);
                toFollow.Add(f);
            }

            await using var ctx = _factory.CreateDbContext();
            await ctx.Followers.AddRangeAsync(toFollow, stoppingToken);
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }
}