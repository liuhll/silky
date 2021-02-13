using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Lms.Rpc.Address.Selector;
using Lms.Rpc.Messages;
using Lms.Rpc.Runtime.Server;
using Polly;

namespace Lms.Rpc.Runtime.Client
{
    public class DefaultRemoteServiceExecutor : IRemoteServiceExecutor
    {
        private readonly IRemoteServiceInvoker _remoteServiceInvoker;
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public DefaultRemoteServiceExecutor(IRemoteServiceInvoker remoteServiceInvoker,
            IServiceEntryLocator serviceEntryLocator)
        {
            _remoteServiceInvoker = remoteServiceInvoker;
            _serviceEntryLocator = serviceEntryLocator;
        }

        public async Task<object> Execute(string serviceId, IList<object> parameters)
        {
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceId);
            if (serviceEntry.IsLocal)
            {
                return serviceEntry.Executor(null, parameters);
            }

            return await Execute(serviceEntry, parameters);
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, IList<object> parameters)
        {
            // todo 1. 失败重试 2. 缓存拦截 3.确定返回值 
            var remoteInvokeMessage = new RemoteInvokeMessage()
            {
                ServiceId = serviceEntry.ServiceDescriptor.Id,
                Parameters = parameters,
            };
            string hashKey = null;
            if (serviceEntry.GovernanceOptions.ShuntStrategy == AddressSelectorMode.HashAlgorithm)
            {
                hashKey = serviceEntry.GetHashKeyValue(parameters.ToArray());
            }

            IAsyncPolicy<object> executePolicy = Policy<object>.Handle<TimeoutException>()
                    .Or<CommunicatonException>()
                    .RetryAsync(serviceEntry.GovernanceOptions.FailoverCount)
                ;
            if (serviceEntry.FallBackExecutor != null)
            {
                var dictParams = serviceEntry.CreateDictParameters(parameters.ToArray());
                var fallbackPolicy = Policy<object>.Handle<LmsException>()
                    .FallbackAsync<object>(serviceEntry.FallBackExecutor(new object[] {dictParams}).GetAwaiter()
                        .GetResult());
                executePolicy = Policy.WrapAsync(executePolicy, fallbackPolicy);
            }

            return await executePolicy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteServiceInvoker.Invoke(remoteInvokeMessage, serviceEntry.GovernanceOptions,
                            hashKey);
                    return invokeResult.Result;
                });
        }
    }
}