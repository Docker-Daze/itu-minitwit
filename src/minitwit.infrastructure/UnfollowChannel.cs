using System.Threading.Channels;
using minitwit.core;

namespace minitwit.infrastructure
{
    public interface IUnfollowChannel
    {
        Channel<Follower> Channel { get; }
    }

    public class UnfollowChannel : IUnfollowChannel
    {
        public Channel<Follower> Channel { get; } = global::System.Threading.Channels.Channel.CreateUnbounded<Follower>();
    }
}
