using System;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core.Exceptions;
using Polly;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.MiniProfiler;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRemoteExecutor : IRemoteExecutor
    {
        private readonly IRemoteInvoker _remoteInvoker;
        private readonly IFallbackInvoker _fallbackInvoker;

        public DefaultRemoteExecutor(IRemoteInvoker remoteInvoker,
            IFallbackInvoker fallbackInvoker)
        {
            _remoteInvoker = remoteInvoker;
            _fallbackInvoker = fallbackInvoker;
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceEntryId = serviceEntry.ServiceEntryDescriptor.Id,
                ServiceId = serviceEntry.ServiceId,
                Parameters = parameters,
            };
            string hashKey = null;
            if (serviceEntry.GovernanceOptions.ShuntStrategy == AddressSelectorMode.HashAlgorithm)
            {
                hashKey = serviceEntry.GetHashKeyValue(parameters.ToArray());
                MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"hashKey is :{hashKey}");
            }

            IAsyncPolicy<object> policy = Policy.NoOpAsync<object>();

            if (serviceEntry.GovernanceOptions.FailoverCount > 0)
            {
                policy.WrapAsync(Policy<object>
                    .Handle<CommunicatonException>()
                    .RetryAsync(serviceEntry.GovernanceOptions.FailoverCount)
                );
            }

            if (serviceEntry.GovernanceOptions.ExecutionTimeoutMillSeconds > 0)
            {
                policy.WrapAsync(Policy.TimeoutAsync(
                    TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.ExecutionTimeoutMillSeconds)));
            }

            if (serviceEntry.FallbackMethodExecutor != null && serviceEntry.FallbackProvider != null)
            {
                var fallbackPolicy = Policy<object>.Handle<SilkyException>(ex =>
                    {
                        var isNotNeedFallback = (ex is INotNeedFallback);
                        if (isNotNeedFallback)
                        {
                            return false;
                        }

                        if (ex is BusinessException)
                        {
                            return serviceEntry.FallbackProvider?.ValidWhenBusinessException == true;
                        }

                        return true;
                    })
                    .FallbackAsync(
                        async (ctx, t) => await _fallbackInvoker.Invoke(serviceEntry, parameters),
                        async (ex, t) =>
                        {
                            // todo When the service is downgraded, notify the responsible person through the early warning system
                        });

                policy = fallbackPolicy.WrapAsync(policy);
            }

            var result = await policy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteInvoker.Invoke(remoteInvokeMessage, serviceEntry.GovernanceOptions,
                            hashKey);
                    return invokeResult.GetResult();
                });


            return result;
        }
    }
}