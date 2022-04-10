using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class OverflowServerHandleFailoverPolicyProvider : InvokeFailoverPolicyProviderBase
    {
        private readonly ILogger<OverflowServerHandleFailoverPolicyProvider> _logger;
        private readonly IServerManager _serverManager;

        public OverflowServerHandleFailoverPolicyProvider(ILogger<OverflowServerHandleFailoverPolicyProvider> logger,
            IServerManager serverManager)
        {
            _logger = logger;
            _serverManager = serverManager;
        }

        public override IAsyncPolicy<object> Create(string serviceEntryId)
        {
            IAsyncPolicy<object> policy = null;
            var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(serviceEntryId);

            if (serviceEntryDescriptor?.GovernanceOptions.RetryTimes > 0)
            {
                policy = Policy<object>
                    .Handle<OverflowMaxServerHandleException>()
                    .Or<SilkyException>(ex => ex.GetExceptionStatusCode() == StatusCode.OverflowMaxServerHandle)
                    .WaitAndRetryAsync(serviceEntryDescriptor.GovernanceOptions.RetryTimes,
                        retryAttempt =>
                            TimeSpan.FromMilliseconds(serviceEntryDescriptor.GovernanceOptions
                                .RetryIntervalMillSeconds),
                        (outcome, timeSpan, retryNumber, context)
                            => OnRetry(retryNumber, outcome, context));
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

        public override event RpcInvokeFailoverHandle OnInvokeFailover;

        public override FailoverType FailoverType { get; } = FailoverType.OverflowServerHandle;
    }
}