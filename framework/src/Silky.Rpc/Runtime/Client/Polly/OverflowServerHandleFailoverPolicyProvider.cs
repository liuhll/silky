using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class OverflowServerHandleFailoverPolicyProvider : InvokeFailoverPolicyProviderBase
    {
        private readonly ILogger<OverflowServerHandleFailoverPolicyProvider> _logger;

        public OverflowServerHandleFailoverPolicyProvider(ILogger<OverflowServerHandleFailoverPolicyProvider> logger)
        {
            _logger = logger;
        }

        public override IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters)
        {
            IAsyncPolicy<object> policy = null;
            if (serviceEntry.GovernanceOptions.RetryTimes > 0)
            {
                if (serviceEntry.GovernanceOptions.RetryIntervalMillSeconds > 0)
                {
                    policy = Policy<object>
                        .Handle<OverflowMaxServerHandleException>()
                        .Or<SilkyException>(ex => ex.GetExceptionStatusCode() == StatusCode.OverflowMaxServerHandle)
                        .WaitAndRetryAsync(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds,
                            retryAttempt =>
                                TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds),
                            (outcome, timeSpan, retryNumber, context)
                                => OnRetry(retryNumber, outcome, context));
                }
                else
                {
                    policy = Policy<object>
                        .Handle<OverflowMaxServerHandleException>()
                        .Or<SilkyException>(ex => ex.GetExceptionStatusCode() == StatusCode.OverflowMaxServerHandle)
                        .RetryAsync(serviceEntry.GovernanceOptions.RetryTimes,
                            onRetryAsync: (outcome, retryNumber, context) =>
                                OnRetry(retryNumber, outcome, context));
                }
            }

            return policy;
        }

        private async Task OnRetry(int retryNumber, DelegateResult<object> outcome, Context context)
        {
            var serviceAddressModel = GetSelectedServerEndpoint();
            _logger.LogWarning(
                $"The maximum concurrent processing capacity allowed by the service provider is exceeded," +
                $" and the rpc call is retryed for the ({retryNumber})th time");
            if (OnInvokeFailover != null)
            {
                await OnInvokeFailover.Invoke(outcome, retryNumber, context, serviceAddressModel,
                    FailoverType.Communication);
            }
        }


        protected virtual IRpcEndpoint GetSelectedServerEndpoint()
        {
            var selectedHost = RpcContext.Context.GetSelectedServerHost();
            if (selectedHost == null)
            {
                return null;
            }

            var selectedServerPort = RpcContext.Context.GetSelectedServerPort();
            var selectedServerServiceProtocol = RpcContext.Context.GetSelectedServerServiceProtocol();
            var selectedServerEndpoint =
                RpcEndpointHelper.CreateRpcEndpoint(selectedHost, selectedServerPort, selectedServerServiceProtocol);
            return selectedServerEndpoint;
        }

        public override event RpcInvokeFailoverHandle OnInvokeFailover;

        public override FailoverType FailoverType { get; } = FailoverType.OverflowServerHandle;
    }
}