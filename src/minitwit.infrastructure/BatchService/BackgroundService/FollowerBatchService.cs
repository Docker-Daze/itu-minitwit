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

public class FollowerBatchService : BackgroundService
{
    private const int BatchSize = 10;
    private readonly Channel<string[]> _chan;
    private readonly IServiceScopeFactory _scopeFactory;

    public FollowerBatchService(Channel<string[]> chan, IServiceScopeFactory scopeFactory)
    {
        _chan = chan;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // collect a full batch of follow events
            var toFollow = new List<Follower>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                var followRequest = await _chan.Reader.ReadAsync(stoppingToken);
                try
                {
                    using var itemScope = _scopeFactory.CreateScope();
                    var userRepo = itemScope.ServiceProvider.GetRequiredService<IUserRepository>();
                    var request = await userRepo.FollowUser(followRequest[0], followRequest[1]);
                    toFollow.Add(request);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "User {User} tried to follow {Target} but something went wrong", followRequest[0], followRequest[1]);
                    continue;
                }
            }
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
            await ctx.Followers.AddRangeAsync(toFollow, stoppingToken);
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }
}