using System.Threading.Channels;
using minitwit.core;

namespace minitwit.infrastructure
{
    public interface IFollowChannel
    {
        Channel<Follower> Channel { get; }
    }

    public class FollowChannel : IFollowChannel
    {
        public Channel<Follower> Channel { get; } = global::System.Threading.Channels.Channel.CreateUnbounded<Follower>();
    }
}
