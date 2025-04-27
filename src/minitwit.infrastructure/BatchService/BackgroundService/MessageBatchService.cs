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
    private readonly IDbContextFactory<MinitwitDbContext> _factory;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;

    public MessageBatchService(
        Channel<string[]> chan,
        IDbContextFactory<MinitwitDbContext> factory, IUserRepository userRepository, IMessageRepository messageRepository)
    {
        _chan = chan;
        _factory = factory;
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // collect a full batch of messages
            var buffer = new List<Message>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                try
                {
                    var att = await _chan.Reader.ReadAsync(stoppingToken);
                    var user = await _userRepository.GetUserFromUsername(att[0]);
                    if (user == null)
                    {
                        throw new Exception("User not found");
                    }
                    var msg = await _messageRepository.AddMessage(user, att[1], int.Parse(att[2]));
                    buffer.Add(msg);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Could not add message to database");
                    continue;
                }
            }

            await using var ctx = _factory.CreateDbContext();

            // unify user instances
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