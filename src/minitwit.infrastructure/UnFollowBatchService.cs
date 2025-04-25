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
        Channel<Follower> chan,
        IDbContextFactory<MinitwitDbContext> factory)
    {
        _chan    = chan;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var toUnfollow = new List<Follower>();

        while (!stoppingToken.IsCancellationRequested)
        {
            // You could use two channels or embed an “action” in Follower,
            // but here we assume two separate services/chan registrations.

            // wait for follow events
            var f = await _chan.Reader.ReadAsync(stoppingToken);
            toUnfollow.Add(f);

            while (toUnfollow.Count < BatchSize &&
                   _chan.Reader.TryRead(out var more))
            {
                toUnfollow.Add(more);
            }

            await using var ctx = _factory.CreateDbContext();
            ctx.Followers.RemoveRange(toUnfollow);
            await ctx.SaveChangesAsync(stoppingToken);

            toUnfollow.Clear();
        }
    }
}