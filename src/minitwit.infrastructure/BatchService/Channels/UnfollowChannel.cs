using System.Threading.Channels;
using minitwit.core;

namespace minitwit.infrastructure
{
    public interface IUnfollowChannel
    {
        Channel<string[]> Channel { get; }
    }

    public class UnfollowChannel : IUnfollowChannel
    {
        public Channel<string[]> Channel { get; } = global::System.Threading.Channels.Channel.CreateUnbounded<string[]>();
    }
}
