using System;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Proxy
{
    public class RpcClientProxyInterceptor : SilkyInterceptor, ITransientDependency
    {
        private readonly IIdGenerator _idGenerator;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly IServiceKeyExecutor _serviceKeyExecutor;
        private readonly IExecutor _executor;

        public RpcClientProxyInterceptor(IIdGenerator idGenerator,
            IServiceEntryLocator serviceEntryLocator,
            IServiceKeyExecutor serviceKeyExecutor,
            IExecutor executor)
        {
            _idGenerator = idGenerator;
            _serviceEntryLocator = serviceEntryLocator;
            _serviceKeyExecutor = serviceKeyExecutor;
            _executor = executor;
        }

        public override async Task InterceptAsync(ISilkyMethodInvocation invocation)
        {
            var serviceEntryId = _idGenerator.GetDefaultServiceEntryId(invocation.Method);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
            try
            {
                if (invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType.GenericTypeArguments.IsNullOrEmpty())
                {
                    await _executor.Execute(serviceEntry, invocation.Arguments, _serviceKeyExecutor.ServiceKey);
                }
                else
                {
                    invocation.ReturnValue =
                        await _executor.Execute(serviceEntry, invocation.Arguments, _serviceKeyExecutor.ServiceKey);
                }


            }
            catch (Exception)
            {
                if (serviceEntry.FallbackMethodExecutor != null && serviceEntry.FallbackProvider != null)
                {
                    await invocation.ProceedAsync();
                }

                throw;
            }
        }
    }
}