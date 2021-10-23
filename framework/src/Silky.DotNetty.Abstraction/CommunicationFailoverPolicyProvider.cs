using System;
using System.IO;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Silky.DotNetty.Abstraction
{
    public class CommunicationFailoverPolicyProvider : InvokeFailoverPolicyProviderBase
    {
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public CommunicationFailoverPolicyProvider(IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
        }

        public override IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters)
        {
            IAsyncPolicy<object> policy = null;
            if (serviceEntry.GovernanceOptions.RetryTimes > 0)
            {
                if (serviceEntry.GovernanceOptions.RetryIntervalMillSeconds > 0)
                {
                    policy = Policy<object>
                        .Handle<ChannelException>()
                        .Or<ConnectException>()
                        .Or<IOException>()
                        .Or<CommunicationException>()
                        .Or<SilkyException>(ex => ex.GetExceptionStatusCode() == StatusCode.NotFindLocalServiceEntry)
                        .WaitAndRetryAsync(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds,
                            retryAttempt =>
                                TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds),
                            async (outcome, timeSpan, retryNumber, context)
                                => await SetInvokeCurrentSeverDisEnable(outcome, retryNumber, context, serviceEntry)
                        );
                }
                else
                {
                    policy = Policy<object>.Handle<ChannelException>()
                        .Or<ConnectException>()
                        .Or<IOException>()
                        .Or<CommunicationException>()
                        .RetryAsync(serviceEntry.GovernanceOptions.RetryTimes,
                            onRetryAsync: async (outcome, retryNumber, context) =>
                                await SetInvokeCurrentSeverDisEnable(outcome, retryNumber, context, serviceEntry));
                }
            }

            return policy;
        }

        private async Task SetInvokeCurrentSeverDisEnable(DelegateResult<object> outcome, int retryNumber,
            Context context, ServiceEntry serviceEntry)
        {
            var serviceAddressModel = GetSelectedServerEndpoint();
            _rpcEndpointMonitor.ChangeStatus(serviceAddressModel, false,
                serviceEntry.GovernanceOptions.UnHealthAddressTimesAllowedBeforeRemoving);
            if (OnInvokeFailover != null)
            {
                await OnInvokeFailover.Invoke(outcome, retryNumber, context, serviceAddressModel,
                    FailoverType.Communication);
            }
        }

        public override event RpcInvokeFailoverHandle OnInvokeFailover;

        public override FailoverType FailoverType { get; } = FailoverType.Communication;
    }
}