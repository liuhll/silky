using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Proxy.Interceptors
{
    public class FallbackInterceptor : LmsInterceptor, ITransientDependency
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public FallbackInterceptor(IServiceIdGenerator serviceIdGenerator, IServiceEntryLocator serviceEntryLocator)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _serviceIdGenerator = serviceIdGenerator;
        }

        public override async Task InterceptAsync(ILmsMethodInvocation invocation)
        {
            var servcieId = _serviceIdGenerator.GenerateServiceId(invocation.Method);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(servcieId);

            var parms = new object[] {serviceEntry.CreateDictParameters(invocation.Arguments)};
            var fallbackResult = await serviceEntry.FallBackExecutor(parms);
            invocation.ReturnValue = fallbackResult;
        }
    }
}