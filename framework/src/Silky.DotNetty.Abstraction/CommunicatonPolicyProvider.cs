using DotNetty.Transport.Channels;
using Polly;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.DotNetty.Abstraction
{
    public class CommunicatonPolicyWithResultProvider : IPolicyWithResultProvider
    {
        public IAsyncPolicy<object> Create(ServiceEntry serviceEntry)
        {
            if (serviceEntry.GovernanceOptions.FailoverCount > 0)
            {
                return Policy<object>.Handle<ChannelException>()
                    .Or<ConnectException>()
                    .RetryAsync(serviceEntry.GovernanceOptions.FailoverCount);
            }

            return null;
        }
    }
}