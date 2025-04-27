namespace minitwit.infrastructure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using minitwit.core;
using minitwit.infrastructure;

public class MessageBatchService : BackgroundService
{
    private const int BatchSize = 10;
    private readonly Channel<Message> _chan;
    private readonly IDbContextFactory<MinitwitDbContext> _factory;

    public MessageBatchService(
        Channel<Message> chan,
        IDbContextFactory<MinitwitDbContext> factory)
    {
        _chan = chan;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // collect a full batch of messages
            var buffer = new List<Message>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                var msg = await _chan.Reader.ReadAsync(stoppingToken);
                buffer.Add(msg);
            }

            // 2c) Write the batch in one go
            await using var ctx = _factory.CreateDbContext();
            // unify user instances to prevent tracking conflicts
            var userMap = new Dictionary<string, User>();
            foreach (var msg in buffer)
            {
                if (msg.User == null) continue;
                if (userMap.TryGetValue(msg.User.Id, out var existing))
                    msg.User = existing;
                else
                    userMap[msg.User.Id] = msg.User;
            }
            foreach (var user in userMap.Values)
                ctx.Entry(user).State = EntityState.Unchanged;
            await ctx.Messages.AddRangeAsync(buffer, stoppingToken);
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }
}