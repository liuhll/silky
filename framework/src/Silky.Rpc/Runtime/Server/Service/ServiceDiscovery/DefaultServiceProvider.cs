using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Extensions;
using Silky.Core.Reflection;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceProvider : IServiceProvider
    {
        private readonly ITypeFinder _typeFinder;

        private readonly IServiceGenerator _serviceGenerator;
        public ILogger<DefaultServiceProvider> Logger { get; set; }

        public DefaultServiceProvider(ITypeFinder typeFinder,
            IServiceGenerator serviceGenerator)
        {
            _typeFinder = typeFinder;
            _serviceGenerator = serviceGenerator;
            Logger = NullLogger<DefaultServiceProvider>.Instance;
        }


        public IReadOnlyCollection<Service> GetServices()
        {
            var serviceTypes = ServiceHelper.FindAllServiceTypes(_typeFinder);
            if (!EngineContext.Current.IsContainHttpCoreModule())
            {
                serviceTypes = serviceTypes.Where(p =>
                    p.Item1.GetCustomAttributes().OfType<DashboardAppServiceAttribute>().FirstOrDefault() == null);
            }

            var services = new List<Service>();
            foreach (var serviceTypeInfo in serviceTypes)
            {
                services.Add(_serviceGenerator.CreateService(serviceTypeInfo));
            }

            if (EngineContext.Current.IsContainWebSocketModule())
            {
                var wsServiceTypes = ServiceHelper.FindServiceLocalWsTypes(_typeFinder);
                foreach (var wsServiceType in wsServiceTypes)
                {
                    services.Add(_serviceGenerator.CreateWsService(wsServiceType));
                }
            }
            
            return services;
        }
    }
}