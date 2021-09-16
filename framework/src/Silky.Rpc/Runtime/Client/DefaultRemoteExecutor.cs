using System.Linq;
using System.Threading.Tasks;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.MiniProfiler;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public class DefaultRemoteExecutor : IRemoteExecutor
    {
        private readonly IRemoteInvoker _remoteInvoker;
        private readonly IPolicyBuilder _policyBuilder;

        public DefaultRemoteExecutor(IRemoteInvoker remoteInvoker,
            IPolicyBuilder policyBuilder)
        {
            _remoteInvoker = remoteInvoker;
            _policyBuilder = policyBuilder;
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

            var policy = _policyBuilder.Build(serviceEntry, parameters);
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