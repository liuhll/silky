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
        private readonly IServerManager _serverManager;

        public CommunicationFailoverPolicyProvider(IRpcEndpointMonitor rpcEndpointMonitor,
            IServerManager serverManager)
        {
            _rpcEndpointMonitor = rpcEndpointMonitor;
            _serverManager = serverManager;
        }

        public override IAsyncPolicy<object> Create(string serviceEntryId)
        {
            IAsyncPolicy<object> policy = null;
            var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(serviceEntryId);
            if (serviceEntryDescriptor?.GovernanceOptions.RetryTimes > 0)
            {
                policy = Policy<object>
                    .Handle<ChannelException>()
                    .Or<ConnectException>()
                    .Or<ConnectTimeoutException>()
                    .Or<IOException>()
                    .Or<CommunicationException>()
                    .Or<SilkyException>(ex => ex.GetExceptionStatusCode() == StatusCode.NotFindLocalServiceEntry)
                    .WaitAndRetryAsync(serviceEntryDescriptor.GovernanceOptions.RetryTimes,
                        retryAttempt =>
                            TimeSpan.FromMilliseconds(serviceEntryDescriptor.GovernanceOptions
                                .RetryIntervalMillSeconds),
                        async (outcome, timeSpan, retryNumber, context)
                            => await SetInvokeCurrentSeverDisEnable(outcome, retryNumber, context,
                                serviceEntryDescriptor)
                    );
            }

            return policy;
        }

        private async Task SetInvokeCurrentSeverDisEnable(DelegateResult<object> outcome, int retryNumber,
            Context context, ServiceEntryDescriptor serviceEntryDescriptor)
        {
            var serviceAddressModel = GetSelectedServerEndpoint();
            if (outcome.Exception is CommunicationException || outcome.Exception is IOException || outcome.Exception is ChannelException)
            {
                _rpcEndpointMonitor.RemoveRpcEndpoint(serviceAddressModel);
            }
            else
            {
                _rpcEndpointMonitor.ChangeStatus(serviceAddressModel, false,
                    serviceEntryDescriptor.GovernanceOptions.UnHealthAddressTimesAllowedBeforeRemoving);
            }

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