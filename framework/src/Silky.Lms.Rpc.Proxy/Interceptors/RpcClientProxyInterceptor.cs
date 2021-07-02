using System;
using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Rpc.Runtime;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Rpc.Proxy.Interceptors
{
    public class RpcClientProxyInterceptor : LmsInterceptor, ITransientDependency
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

        public async override Task InterceptAsync(ILmsMethodInvocation invocation)
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
