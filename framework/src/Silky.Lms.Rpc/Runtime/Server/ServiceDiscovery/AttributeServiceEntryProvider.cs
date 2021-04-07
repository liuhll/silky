using System.Collections.Generic;
using System.Linq;
using Silky.Lms.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery
{
    public class AttributeServiceEntryProvider : IServiceEntryProvider
    {
        private readonly ITypeFinder _typeFinder;
        private readonly IServiceEntryGenerator _serviceEntryGenerator;
        public ILogger<AttributeServiceEntryProvider> Logger { get; set; }

        public AttributeServiceEntryProvider(ITypeFinder typeFinder,
            IServiceEntryGenerator serviceEntryGenerator)
        {
            _typeFinder = typeFinder;
            _serviceEntryGenerator = serviceEntryGenerator;
            Logger = NullLogger<AttributeServiceEntryProvider>.Instance;;
        }

        public IReadOnlyList<ServiceEntry> GetEntries()
        {
            var serviceEntryTypes = ServiceEntryHelper.FindAllServiceEntryTypes(_typeFinder);
            Logger.LogDebug($"发现了以下服务：{string.Join(",", serviceEntryTypes.Select(i => i.ToString()))}。");
            var entries = new List<ServiceEntry>();
            foreach (var serviceEntryType in serviceEntryTypes)
            {
                entries.AddRange(_serviceEntryGenerator.CreateServiceEntry(serviceEntryType));
            }
            return entries;
        }
    }
}