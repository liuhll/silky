using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceManager : IServiceManager
    {
        private IEnumerable<Service> m_localServices;
        private IEnumerable<Service> m_allServices;

        public DefaultServiceManager(IEnumerable<IServiceProvider> providers)
        {
            UpdateServices(providers);
        }

        private void UpdateServices(IEnumerable<IServiceProvider> providers)
        {
            var allServices = new List<Service>();
            foreach (var provider in providers)
            {
                var services = provider.GetServices();
                allServices.AddRange(services);
            }

            if (allServices.GroupBy(p => p.Id).Any(p => p.Count() > 1))
            {
                throw new SilkyException(
                    "There is duplicate service information, please check the service you set");
            }

            m_allServices = allServices.ToList();
            m_localServices = allServices.Where(p => p.IsLocal).ToList();
        }

        public IReadOnlyList<Service> GetLocalService()
        {
            return m_localServices.ToArray();
        }

        public IReadOnlyList<Service> GetAllService()
        {
            return m_allServices.ToArray();
        }

        public IReadOnlyCollection<string> GetAllApplications()
        {
            return m_localServices.GroupBy(p => p.ServiceDescriptor.Application).Select(p => p.Key).ToArray();
        }

        public bool IsLocalService(string serviceId)
        {
            return m_localServices.Any(p => p.Id == serviceId);
        }

        public Service GetService(string serviceId)
        {
            return m_allServices.FirstOrDefault(p => p.Id == serviceId);
        }

        public void Update(Service service)
        {
            m_allServices = m_allServices
                .Where(p => !p.Id.Equals(service.Id))
                .Append(service);
            if (service.IsLocal)
            {
                m_localServices = m_localServices
                    .Where(p => !p.Id.Equals(service.Id))
                    .Append(service);
            }

            OnUpdate?.Invoke(this, service);
        }

        public event EventHandler<Service> OnUpdate;
    }
}