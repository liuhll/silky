using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    public class DefaultServiceEntryManager : IServiceEntryManager
    {
        private IEnumerable<ServiceEntry> m_localServiceEntries;
        private IEnumerable<ServiceEntry> m_allServiceEntries;

        public DefaultServiceEntryManager(IEnumerable<IServiceEntryProvider> providers)
        {
            UpdateEntries(providers);
        }

        private void UpdateEntries(IEnumerable<IServiceEntryProvider> providers)
        {
            var localServiceEntries = new List<ServiceEntry>();
            var allServiceEntries = new List<ServiceEntry>();
            foreach (var provider in providers)
            {
                var entries = provider.GetEntries();
                foreach (var entry in entries)
                {
                    if (localServiceEntries.Any(p=>p.ServiceDescriptor.Id == entry.ServiceDescriptor.Id))
                    {
                        throw new InvalidOperationException($"本地包含多个Id为：{entry.ServiceDescriptor.Id} 的服务条目。");
                    }
                    localServiceEntries.Add(entry);
                }
            }

            m_localServiceEntries = localServiceEntries;


        }

        public IReadOnlyList<ServiceEntry> GetEntries()
        {
            return m_localServiceEntries.ToImmutableList();
        }
    }
}