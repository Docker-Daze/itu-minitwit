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
        _chan    = chan;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<Message>(BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            // 2a) Wait for at least one message
            var msg = await _chan.Reader.ReadAsync(stoppingToken);
            buffer.Add(msg);

            // 2b) Drain up to BatchSize – 1 more
            while (buffer.Count < BatchSize &&
                   _chan.Reader.TryRead(out var more))
            {
                buffer.Add(more);
            }

            // 2c) Write the batch in one go
            await using var ctx = _factory.CreateDbContext();
            // prevent re-inserting existing users
            foreach (var message in buffer)
            {
                if (message.User != null)
                    ctx.Entry(message.User).State = EntityState.Unchanged;
            }
            await ctx.Messages.AddRangeAsync(buffer, stoppingToken);
            await ctx.SaveChangesAsync(stoppingToken);

            buffer.Clear();
        }
    }
}