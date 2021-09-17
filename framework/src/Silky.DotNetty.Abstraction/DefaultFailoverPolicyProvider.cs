using System;
using System.IO;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Options;
using Polly;
using Silky.Core.Rpc;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Configuration;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.DotNetty.Abstraction
{
    public class DefaultFailoverPolicyProvider : IFailoverPolicyProvider
    {
        private readonly IHealthCheck _healthCheck;
        private GovernanceOptions _governanceOptions;

        public DefaultFailoverPolicyProvider(IHealthCheck healthCheck,
            IOptionsSnapshot<GovernanceOptions> governanceOptions)
        {
            _healthCheck = healthCheck;
            _governanceOptions = governanceOptions.Value;
        }

        public IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters)
        {
            IAsyncPolicy<object> policy = null;
            if (serviceEntry.GovernanceOptions.RetryTimes > 0)
            {
                if (serviceEntry.GovernanceOptions.RetryIntervalMillSeconds > 0)
                {
                    policy = Policy<object>.Handle<ChannelException>()
                        .Or<ConnectException>()
                        .Or<IOException>()
                        .WaitAndRetryAsync(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds,
                            retryAttempt =>
                                TimeSpan.FromMilliseconds(serviceEntry.GovernanceOptions.RetryIntervalMillSeconds),
                            async (outcome, timeSpan, retryNumber, context)
                                => await SetInvokeCurrentSeverUnHealth(outcome, retryNumber, context)
                        );
                }
                else
                {
                    policy = Policy<object>.Handle<ChannelException>()
                        .Or<ConnectException>()
                        .Or<IOException>()
                        .RetryAsync(serviceEntry.GovernanceOptions.RetryTimes,
                            onRetryAsync: async (outcome, retryNumber, context) =>
                                await SetInvokeCurrentSeverUnHealth(outcome, retryNumber, context));
                }
            }

            return policy;
        }

        private async Task SetInvokeCurrentSeverUnHealth(DelegateResult<object> outcome, int retryNumber,
            Context context)
        {
            var serverAddress = RpcContext.Context.GetServerAddress();
            var serverServiceProtocol = RpcContext.Context.GetServerServiceProtocol();
            var serviceAddressModel =
                NetUtil.CreateAddressModel(serverAddress, serverServiceProtocol);

            _healthCheck.ChangeHealthStatus(serviceAddressModel, false, _governanceOptions.RemovedUnHealthAddressTimes);
            if (OnInvokeFailover != null)
            {
                await OnInvokeFailover.Invoke(outcome, retryNumber, context, serviceAddressModel);
            }
        }

        public event RpcInvokeFailoverHandle OnInvokeFailover;
    }
}