using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceProvider : IServiceProvider
    {
        private readonly ITypeFinder _typeFinder;
        private readonly IServiceEntryGenerator _serviceEntryGenerator;
        private readonly IServiceGenerator _serviceGenerator;
        public ILogger<DefaultServiceProvider> Logger { get; set; }

        public DefaultServiceProvider(ITypeFinder typeFinder,
            IServiceEntryGenerator serviceEntryGenerator,
            IServiceGenerator serviceGenerator)
        {
            _typeFinder = typeFinder;
            _serviceEntryGenerator = serviceEntryGenerator;
            _serviceGenerator = serviceGenerator;
            Logger = NullLogger<DefaultServiceProvider>.Instance;
            ;
        }

        public IReadOnlyList<ServiceEntry> GetEntries()
        {
            var serviceTypes = ServiceHelper.FindAllServiceTypes(_typeFinder);
            Logger.LogDebug(
                $"The following AppServices were found: {string.Join(",", serviceTypes.Select(i => i.Item1.FullName))}.");
            var entries = new List<ServiceEntry>();
            foreach (var serviceTypeInfo in serviceTypes)
            {
                entries.AddRange(_serviceEntryGenerator.CreateServiceEntry(serviceTypeInfo));
            }

            return entries;
        }

        public IReadOnlyCollection<Service> GetServices()
        {
            var serviceTypes = ServiceHelper.FindAllServiceTypes(_typeFinder);
            Logger.LogDebug(
                $"The following AppServices were found: {string.Join(",", serviceTypes.Select(i => i.Item1.FullName))}.");
            var services = new List<Service>();
            foreach (var serviceTypeInfo in serviceTypes)
            {
                services.Add(_serviceGenerator.CreateService(serviceTypeInfo));
            }

            var wsServiceTypes = ServiceHelper.FindServiceLocalWsTypes(_typeFinder);
            foreach (var wsServiceType in wsServiceTypes)
            {
                services.Add(_serviceGenerator.CreateWsService(wsServiceType));
            }

            return services;
        }
    }
}