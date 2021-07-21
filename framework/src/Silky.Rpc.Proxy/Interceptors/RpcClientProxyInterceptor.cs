using System;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Proxy.Interceptors
{
    public class RpcClientProxyInterceptor : SilkyInterceptor, ITransientDependency
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly ICurrentServiceKey _currentServiceKey;
        private readonly IServiceExecutor _serviceExecutor;

        public RpcClientProxyInterceptor(IServiceIdGenerator serviceIdGenerator,
            IServiceEntryLocator serviceEntryLocator,
            ICurrentServiceKey currentServiceKey,
            IServiceExecutor serviceExecutor)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _serviceEntryLocator = serviceEntryLocator;
            _currentServiceKey = currentServiceKey;
            _serviceExecutor = serviceExecutor;
        }

        public async override Task InterceptAsync(ISilkyMethodInvocation invocation)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(invocation.Method);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(serviceId);
            try
            {
                invocation.ReturnValue =
                    await _serviceExecutor.Execute(serviceEntry, invocation.Arguments, _currentServiceKey.ServiceKey);
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
