using System;
using System.Collections.Generic;
using System.Linq;
using Lms.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    public class AttributeServiceEntryProvider : IServiceEntryProvider
    {
        private readonly ITypeFinder _typeFinder;
        private readonly IClrServiceEntryFactory _clrServiceEntryFactory;
        private readonly ILogger<AttributeServiceEntryProvider> _logger;

        public AttributeServiceEntryProvider(ITypeFinder typeFinder,
            IClrServiceEntryFactory clrServiceEntryFactory)
        {
            _typeFinder = typeFinder;
            _clrServiceEntryFactory = clrServiceEntryFactory;
            _logger = NullLogger<AttributeServiceEntryProvider>.Instance;;
        }

        public IReadOnlyList<ServiceEntry> GetEntries()
        {
            var serviceEntryTypes = ServiceEntryHelper.FindAllServiceEntryTypes(_typeFinder);
            _logger.LogDebug($"发现了以下服务：{string.Join(",", serviceEntryTypes.Select(i => i.ToString()))}。");
            var entries = new List<ServiceEntry>();
            foreach (var serviceEntryType in serviceEntryTypes)
            {
                entries.AddRange(_clrServiceEntryFactory.CreateServiceEntry(serviceEntryType));
            }
            return entries;
        }
    }
}