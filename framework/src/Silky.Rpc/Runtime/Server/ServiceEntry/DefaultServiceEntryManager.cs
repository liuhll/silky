using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceEntryManager : IServiceEntryManager
    {
        private IEnumerable<ServiceEntry> m_localServiceEntries;
        private IEnumerable<ServiceEntry> m_allServiceEntries;
        private IChangeToken? _changeToken;

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
                    if (allServiceEntries.Any(p => p.ServiceEntryDescriptor.Id == entry.ServiceEntryDescriptor.Id))
                    {
                        throw new InvalidOperationException(
                            $"Locally contains multiple service entries with Id: {entry.ServiceEntryDescriptor.Id}");
                    }

                    allServiceEntries.Add(entry);
                }
            }

            if (allServiceEntries.GroupBy(p => p.Router).Any(p => p.Count() > 1))
            {
                throw new SilkyException(
                    "There is duplicate routing information, please check the service routing you set");
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

        public IReadOnlyCollection<ServiceEntry> GetServiceEntries(string serviceId)
        {
            return m_allServiceEntries.Where(p => p.ServiceId == serviceId).ToArray();
        }

        public ServiceEntry GetServiceEntry(string serviceEntryId)
        {
            return m_allServiceEntries.FirstOrDefault(p => p.Id == serviceEntryId);
        }

        public bool HasHttpProtocolServiceEntry()
        {
            return m_allServiceEntries.Any(p => p.NeedHttpProtocolSupport());
        }

        public event EventHandler<ServiceEntry> OnUpdate;


        public void Update(ServiceEntry serviceEntry)
        {
            m_allServiceEntries = m_allServiceEntries
                .Where(p => !p.ServiceEntryDescriptor.Id.Equals(serviceEntry.ServiceEntryDescriptor.Id))
                .Append(serviceEntry);
            if (serviceEntry.IsLocal)
            {
                m_localServiceEntries = m_localServiceEntries
                    .Where(p => !p.ServiceEntryDescriptor.Id.Equals(serviceEntry.ServiceEntryDescriptor.Id))
                    .Append(serviceEntry);
            }

            OnUpdate?.Invoke(this, serviceEntry);
        }
    }
}