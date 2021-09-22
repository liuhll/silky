using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceEntryProvider : IServiceEntryProvider
    {
        public ILogger<DefaultServiceEntryProvider> Logger { get; set; }
        private readonly IServiceEntryGenerator _serviceEntryGenerator;
        private readonly ITypeFinder _typeFinder;

        public DefaultServiceEntryProvider(IServiceEntryGenerator serviceEntryGenerator,
            ITypeFinder typeFinder)
        {
            _serviceEntryGenerator = serviceEntryGenerator;
            _typeFinder = typeFinder;
            Logger = NullLogger<DefaultServiceEntryProvider>.Instance;
        }

        public IReadOnlyList<ServiceEntry> GetEntries()
        {
            var serviceTypeInfos = ServiceHelper.FindAllServiceTypes(_typeFinder);
            Logger.LogDebug(
                $"The following Services were found:{Environment.NewLine}" +
                $"{string.Join($",{Environment.NewLine}", serviceTypeInfos.Select(i => $"Type:{i.Item1.FullName}-->IsLocal:{i.Item2.ToString()}"))}.");
            var entries = new List<ServiceEntry>();
            foreach (var serviceTypeInfo in serviceTypeInfos)
            {
                Logger.LogDebug(
                    $"Prepare to generate the service entry for the service [{serviceTypeInfo.Item1.FullName}]");
                entries.AddRange(_serviceEntryGenerator.CreateServiceEntry(serviceTypeInfo));
            }

            return entries;
        }
    }
}