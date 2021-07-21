using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Runtime.Server
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
            var allServiceEntries = new List<ServiceEntry>();
            foreach (var provider in providers)
            {
                var entries = provider.GetEntries();
                foreach (var entry in entries)
                {
                    if (allServiceEntries.Any(p => p.ServiceDescriptor.Id == entry.ServiceDescriptor.Id))
                    {
                        throw new InvalidOperationException($"本地包含多个Id为：{entry.ServiceDescriptor.Id} 的服务条目。");
                    }

                    allServiceEntries.Add(entry);
                }
            }

            if (allServiceEntries.GroupBy(p => p.Router).Any(p => p.Count() > 1))
            {
                throw new SilkyException("存在重复的路由信息,请检查您设置的服务路由");
            }

            m_allServiceEntries = allServiceEntries;
            m_localServiceEntries = allServiceEntries.Where(p => p.IsLocal);
        }

        public IReadOnlyList<ServiceEntry> GetLocalEntries()
        {
            return m_localServiceEntries.ToImmutableList();
        }

        public IReadOnlyList<ServiceEntry> GetAllEntries()
        {
            return m_allServiceEntries.ToImmutableList();
        }

        public event EventHandler<ServiceEntry> OnUpdate;

        public void Update(ServiceEntry serviceEntry)
        {
            m_allServiceEntries = m_allServiceEntries
                .Where(p => !p.ServiceDescriptor.Id.Equals(serviceEntry.ServiceDescriptor.Id)).Append(serviceEntry);
            if (serviceEntry.IsLocal)
            {
                m_localServiceEntries = m_localServiceEntries
                    .Where(p => !p.ServiceDescriptor.Id.Equals(serviceEntry.ServiceDescriptor.Id)).Append(serviceEntry);
            }
            OnUpdate?.Invoke(this, serviceEntry);
        }
    }
}