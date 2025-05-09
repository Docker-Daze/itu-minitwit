namespace minitwit.infrastructure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using minitwit.core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;

public class MessageBatchService : BackgroundService
{
    private const int BatchSize = 10;
    private readonly Channel<string[]> _chan;
    private readonly IServiceScopeFactory _scopeFactory;

    public MessageBatchService(
        Channel<string[]> chan, IServiceScopeFactory scopeFactory)
    {
        _chan = chan;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // collect a full batch of messages
            var buffer = new List<Message>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                var att = await _chan.Reader.ReadAsync(stoppingToken);
                try
                {
                    using var itemScope = _scopeFactory.CreateScope();
                    var userRepo = itemScope.ServiceProvider.GetRequiredService<IUserRepository>();
                    var msgRepo  = itemScope.ServiceProvider.GetRequiredService<IMessageRepository>();
                    var msg = await ValidateMessage(att, userRepo, msgRepo);
                    buffer.Add(msg);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Could not add message from {username} to database", att[0]);
                    continue;
                }
            }

            // unify user instances
            var userMap = MakeUserDict(buffer);
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
            
            foreach (var user in userMap.Values)
                ctx.Entry(user).State = EntityState.Unchanged;
            
            await ctx.Messages.AddRangeAsync(buffer, stoppingToken);
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task<Message> ValidateMessage(string[] att, IUserRepository userRepository, IMessageRepository messageRepository)
    {
        var user = await userRepository.GetUserFromUsername(att[0]);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        return await messageRepository.AddMessage(user, att[1], int.Parse(att[2]));
    }

    private Dictionary<string, User> MakeUserDict(List<Message> messages)
    {
        var userMap = new Dictionary<string, User>();
        foreach (var msg in messages)
        {
            if (msg.User == null) continue;
            if (userMap.TryGetValue(msg.User.Id, out var existing))
                msg.User = existing;
            else
                userMap[msg.User.Id] = msg.User;
        }

        return userMap;
    }
}