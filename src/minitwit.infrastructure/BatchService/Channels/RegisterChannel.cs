using System.Threading.Channels;
using minitwit.core;

namespace minitwit.infrastructure
{
    public interface IRegisterChannel
    {
        Channel<RegisterRequest> Channel { get; }
    }

    public class RegisterChannel : IRegisterChannel
    {
        public Channel<RegisterRequest> Channel { get; } = global::System.Threading.Channels.Channel.CreateUnbounded<RegisterRequest>();
    }
}
