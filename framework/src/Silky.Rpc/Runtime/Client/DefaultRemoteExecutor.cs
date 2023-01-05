using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client
{
    internal class DefaultRemoteExecutor : IRemoteExecutor
    {
        private readonly IRemoteCaller _remoteCaller;
        private readonly IInvokePolicyBuilder _invokePolicyBuilder;
        private readonly IFileParameterConverter _fileParameterConverter;

        private readonly ConcurrentDictionary<string, IAsyncPolicy<object>> _policyCaches = new();

        public ILogger<DefaultRemoteExecutor> Logger { get; set; }

        public DefaultRemoteExecutor(IRemoteCaller remoteCaller,
            IInvokePolicyBuilder invokePolicyBuilder,
            IFileParameterConverter fileParameterConverter)
        {
            _remoteCaller = remoteCaller;
            _invokePolicyBuilder = invokePolicyBuilder;
            _fileParameterConverter = fileParameterConverter;
            Logger = NullLogger<DefaultRemoteExecutor>.Instance;
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceEntryId = serviceEntry.ServiceEntryDescriptor.Id,
                ServiceId = serviceEntry.ServiceId,
                Parameters = _fileParameterConverter.Convert(parameters),
                ParameterType = ParameterType.Rpc,
            };
            string hashKey = null;
            if (serviceEntry.GovernanceOptions.ShuntStrategy == ShuntStrategy.HashAlgorithm)
            {
                hashKey = GetHashKeyValue();
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"The value of hashkey corresponding to this rpc request is:[{hashKey}]");
            }

            if (!_policyCaches.TryGetValue(serviceEntry.Id, out var policyObject))
            {
                policyObject = _invokePolicyBuilder.Build(serviceEntry.Id);
                _policyCaches.TryAdd(serviceEntry.Id, policyObject);
            }

            var policy = policyObject.WrapFallbackPolicy(serviceEntry.Id, parameters);
            var result = await policy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteCaller.Invoke(remoteInvokeMessage, serviceEntry.GovernanceOptions.ShuntStrategy,
                            hashKey);
                    return invokeResult.GetResult();
                });
            return result;
        }

        public async Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor, object[] parameters,
            string serviceKey = null)
        {
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceEntryId = serviceEntryDescriptor.Id,
                ServiceId = serviceEntryDescriptor.ServiceId,
                Parameters = _fileParameterConverter.Convert(parameters),
                ParameterType = ParameterType.Rpc
            };
            string hashKey = null;
            if (serviceEntryDescriptor.GovernanceOptions.ShuntStrategy == ShuntStrategy.HashAlgorithm)
            {
                hashKey = GetHashKeyValue();
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"The value of hashkey corresponding to this rpc request is:[{hashKey}]");
            }

            var policy = _invokePolicyBuilder.Build(serviceEntryDescriptor.Id);
            var result = await policy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteCaller.Invoke(remoteInvokeMessage,
                            serviceEntryDescriptor.GovernanceOptions.ShuntStrategy,
                            hashKey);
                    return invokeResult.GetResult();
                });

            return result;
        }

        public async Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor,
            IDictionary<string, object> parameters, string serviceKey = null)
        {
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceEntryId = serviceEntryDescriptor.Id,
                ServiceId = serviceEntryDescriptor.ServiceId,
                DictParameters = _fileParameterConverter.Convert(parameters),
                ParameterType = ParameterType.Dict
            };
            string hashKey = null;
            if (serviceEntryDescriptor.GovernanceOptions.ShuntStrategy == ShuntStrategy.HashAlgorithm)
            {
                hashKey = GetHashKeyValue();
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"The value of hashkey corresponding to this rpc request is:[{hashKey}]");
            }

            var policy = _invokePolicyBuilder.Build(serviceEntryDescriptor.Id);
            var result = await policy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteCaller.Invoke(remoteInvokeMessage,
                            serviceEntryDescriptor.GovernanceOptions.ShuntStrategy,
                            hashKey);
                    return invokeResult.GetResult();
                });

            return result;
        }

        public async Task<object> Execute(ServiceEntryDescriptor serviceEntryDescriptor,
            IDictionary<ParameterFrom, object> parameters, string serviceKey = null)
        {
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceEntryId = serviceEntryDescriptor.Id,
                ServiceId = serviceEntryDescriptor.ServiceId,
                HttpParameters = _fileParameterConverter.Convert(parameters),
                ParameterType = ParameterType.Http
            };
            string hashKey = null;
            if (serviceEntryDescriptor.GovernanceOptions.ShuntStrategy == ShuntStrategy.HashAlgorithm)
            {
                hashKey = GetHashKeyValue();
                Logger.LogWithMiniProfiler(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"The value of hashkey corresponding to this rpc request is:[{hashKey}]");
            }

            var policy = _invokePolicyBuilder.Build(serviceEntryDescriptor.Id);
            var result = await policy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteCaller.Invoke(remoteInvokeMessage,
                            serviceEntryDescriptor.GovernanceOptions.ShuntStrategy,
                            hashKey);
                    return invokeResult.GetResult();
                });

            return result;
        }

        private string GetHashKeyValue()
        {
            var headers = RpcContext.Context.GetRequestHeader();
            return headers.TryOrdinalIgnoreCaseGetValue(HashShuntStrategyAttribute.HashKey, out var hashKey) ? hashKey : default;
        }
    }
}