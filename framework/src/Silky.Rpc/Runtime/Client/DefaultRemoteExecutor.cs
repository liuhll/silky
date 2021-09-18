using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRemoteExecutor : IRemoteExecutor
    {
        private readonly IRemoteInvoker _remoteInvoker;
        private readonly IInvokePolicyBuilder _invokePolicyBuilder;
        public ILogger<DefaultRemoteExecutor> Logger { get; set; }

        public DefaultRemoteExecutor(IRemoteInvoker remoteInvoker,
            IInvokePolicyBuilder invokePolicyBuilder)
        {
            _remoteInvoker = remoteInvoker;
            _invokePolicyBuilder = invokePolicyBuilder;
            Logger = NullLogger<DefaultRemoteExecutor>.Instance;
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
            if (serviceEntry.GovernanceOptions.ShuntStrategy == ShuntStrategy.HashAlgorithm)
            {
                hashKey = serviceEntry.GetHashKeyValue(parameters.ToArray());
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"The value of hashkey corresponding to this rpc request is:[{hashKey}]");
            }

            var policy = _invokePolicyBuilder.Build(serviceEntry, parameters);
            var result = await policy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteInvoker.Invoke(remoteInvokeMessage, serviceEntry.GovernanceOptions.ShuntStrategy,
                            hashKey);
                    return invokeResult.GetResult();
                });

            return result;
        }
    }
}