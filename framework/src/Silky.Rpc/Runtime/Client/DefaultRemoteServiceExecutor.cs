using System;
using System.Linq;
using System.Threading.Tasks;
using Silky.Core.Exceptions;
using Polly;
using Silky.Core;
using Silky.Rpc.Address.Selector;
using Silky.Rpc.Messages;
using Silky.Rpc.MiniProfiler;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
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
                MiniProfilerPrinter.Print(MiniProfileConstant.Rpc.Name, MiniProfileConstant.Rpc.State.HashKey,
                    $"hashKey is :{hashKey}");
            }

            IAsyncPolicy<object> executePolicy = Policy<object>
                    .Handle<TimeoutException>()
                    .Or<CommunicatonException>()
                    .Or<OverflowMaxRequestException>()
                    .Or<NotFindLocalServiceEntryException>()
                    .RetryAsync(serviceEntry.GovernanceOptions.FailoverCount)
                ;
            if (serviceEntry.FallBackExecutor != null)
            {
                var dictParams = serviceEntry.CreateDictParameters(parameters.ToArray());
                var fallbackPolicy = Policy<object>.Handle<SilkyException>(
                        ex => !ex.IsBusinessException()
                              && !(ex is CommunicatonException)
                              && !(ex is OverflowMaxRequestException)
                    )
                    .FallbackAsync<object>(serviceEntry.FallBackExecutor(new object[] {dictParams}).GetAwaiter()
                        .GetResult());
                executePolicy = Policy.WrapAsync(executePolicy, fallbackPolicy);
            }

            var filters = EngineContext.Current.ResolveAll<IClientFilter>().OrderBy(p => p.Order).ToArray();
            var rpcActionExcutingContext = new ServiceEntryExecutingContext()
            {
                ServiceEntry = serviceEntry,
                Parameters = parameters,
                ServiceKey = serviceKey
            };

            foreach (var filter in filters)
            {
                filter.OnActionExecuting(rpcActionExcutingContext);
            }

            var result = await executePolicy
                .ExecuteAsync(async () =>
                {
                    var invokeResult =
                        await _remoteServiceInvoker.Invoke(remoteInvokeMessage, serviceEntry.GovernanceOptions,
                            hashKey);
                    return invokeResult.GetResult();
                });

            var rpcActionExecutedContext = new ServiceEntryExecutedContext()
            {
                Result = result
            };
            foreach (var filter in filters)
            {
                filter.OnActionExecuted(rpcActionExecutedContext);
            }

            if (rpcActionExecutedContext.Exception != null)
            {
                throw rpcActionExecutedContext.Exception;
            }

            return rpcActionExecutedContext.Result;
        }
    }
}