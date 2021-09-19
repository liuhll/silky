using System;
using System.Threading.Tasks;
using Polly;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public class NonFrameworkExceptionInvokeFailoverPolicyProvider : InvokeFailoverPolicyProviderBase
    {
        public override IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters)
        {
            IAsyncPolicy<object> policy = null;
            if (serviceEntry.GovernanceOptions.RetryTimes > 0)
            {
                if (serviceEntry.GovernanceOptions.RetryIntervalMillSeconds > 0)
                {
                    policy = Policy<object>
                        .Handle<Exception>(ex => ex.GetExceptionStatusCode() == StatusCode.NonSilkyException)
                        .WaitAndRetryAsync(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds,
                            retryAttempt =>
                                TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds),
                            async (outcome, timeSpan, retryNumber, context)
                                => await OnRetry(outcome, retryNumber, context)
                        );
                }
                else
                {
                    policy = Policy<object>.Handle<Exception>(ex =>
                            ex.GetExceptionStatusCode() == StatusCode.NonSilkyException)
                        .RetryAsync(serviceEntry.GovernanceOptions.RetryTimes,
                            onRetryAsync: async (outcome, retryNumber, context) =>
                                await OnRetry(outcome, retryNumber, context));
                }
            }

            return policy;
        }

        private async Task OnRetry(DelegateResult<object> outcome, int retryNumber, Context context)
        {
            var selectedAddress = GetSelectedServerEndpoint();
            if (OnInvokeFailover != null)
            {
                await OnInvokeFailover?.Invoke(outcome, retryNumber, context, selectedAddress, FailoverType);
            }
        }

        public override event RpcInvokeFailoverHandle OnInvokeFailover;

        public override FailoverType FailoverType { get; } = FailoverType.NonSilkyFrameworkException;
    }
}