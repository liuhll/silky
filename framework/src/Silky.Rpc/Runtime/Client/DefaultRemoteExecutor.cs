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
        private readonly IInvokePolicyBuilder _invokePolicyBuilder;

        public DefaultRemoteExecutor(IRemoteInvoker remoteInvoker,
            IInvokePolicyBuilder invokePolicyBuilder)
        {
            _remoteInvoker = remoteInvoker;
            _invokePolicyBuilder = invokePolicyBuilder;
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