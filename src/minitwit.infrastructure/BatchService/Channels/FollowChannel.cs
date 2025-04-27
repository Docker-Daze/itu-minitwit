using System.Threading.Channels;
using minitwit.core;

namespace minitwit.infrastructure
{
    public interface IFollowChannel
    {
        Channel<string[]> Channel { get; }
    }

    public class FollowChannel : IFollowChannel
    {
        public Channel<string[]> Channel { get; } = global::System.Threading.Channels.Channel.CreateUnbounded<string[]>();
    }
}
