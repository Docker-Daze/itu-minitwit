using System.Collections.Concurrent;
using minitwit.core;
using minitwit.infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class MessageBatchService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();

    public MessageBatchService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // Called by your API or repository when a message is posted.
    public void EnqueueMessage(Message message)
    {
        _messageQueue.Enqueue(message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait for 2 seconds or until cancellation
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            // If there are no messages, continue
            if (_messageQueue.IsEmpty)
                continue;

            // Dequeue all messages available at this moment.
            var messagesToInsert = new List<Message>();
            while (_messageQueue.TryDequeue(out var message))
            {
                messagesToInsert.Add(message);
            }

            if (messagesToInsert.Any())
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MinitwitDbContext>();
                    dbContext.Messages.AddRange(messagesToInsert);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}