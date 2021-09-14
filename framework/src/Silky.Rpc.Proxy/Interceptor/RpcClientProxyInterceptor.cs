using System;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Proxy
{
    public class RpcClientProxyInterceptor : SilkyInterceptor, IScopedDependency
    {
        private readonly IIdGenerator _idGenerator;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly ICurrentServiceKey _currentServiceKey;
        private readonly IExecutor _executor;

        public RpcClientProxyInterceptor(IIdGenerator idGenerator,
            IServiceEntryLocator serviceEntryLocator,
            ICurrentServiceKey currentServiceKey,
            IExecutor executor)
        {
            _idGenerator = idGenerator;
            _serviceEntryLocator = serviceEntryLocator;
            _currentServiceKey = currentServiceKey;
            _executor = executor;
        }

        public override async Task InterceptAsync(ISilkyMethodInvocation invocation)
        {
            var serviceEntryId = _idGenerator.GetDefaultServiceEntryId(invocation.Method);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceEntryId);
            try
            {
                invocation.ReturnValue =
                    await _executor.Execute(serviceEntry, invocation.Arguments, _currentServiceKey.ServiceKey);
            }
            catch (Exception e)
            {
                if (!e.IsBusinessException() && serviceEntry.FallBackExecutor != null)
                {
                    await invocation.ProceedAsync();
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
