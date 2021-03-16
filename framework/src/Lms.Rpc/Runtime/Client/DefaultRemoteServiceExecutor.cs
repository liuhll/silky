using System;
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

        public DefaultRemoteServiceExecutor(IRemoteServiceInvoker remoteServiceInvoker)
        {
            _remoteServiceInvoker = remoteServiceInvoker;
        }
        
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
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

            IAsyncPolicy<object> executePolicy = Policy<object>
                    .Handle<TimeoutException>()
                    .Or<CommunicatonException>()
                    .Or<OverflowMaxRequestException>()
                    .RetryAsync(serviceEntry.GovernanceOptions.FailoverCount)
                ;
            if (serviceEntry.FallBackExecutor != null)
            {
                var dictParams = serviceEntry.CreateDictParameters(parameters.ToArray());
                var fallbackPolicy = Policy<object>.Handle<LmsException>(
                        ex => !ex.IsBusinessException()
                              && !(ex is CommunicatonException)
                              && !(ex is OverflowMaxRequestException)
                    )
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