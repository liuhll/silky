using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceManager : IServiceManager
    {
        private IEnumerable<ServiceInfo> m_localServices;
        private IEnumerable<ServiceInfo> m_allServices;

        public DefaultServiceManager(IEnumerable<IServiceProvider> providers)
        {
            UpdateServices(providers);
        }

        private void UpdateServices(IEnumerable<IServiceProvider> providers)
        {
            var allServices = new List<ServiceInfo>();
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

        public IReadOnlyList<ServiceInfo> GetLocalService()
        {
            return m_localServices.ToArray();
        }

        public IReadOnlyList<ServiceInfo> GetAllService()
        {
            return m_allServices.ToArray();
        }

        public bool IsLocalService(string serviceId)
        {
            return m_localServices.Any(p => p.Id == serviceId);
        }

        public ServiceInfo GetService(string serviceId)
        {
            return m_allServices.FirstOrDefault(p => p.Id == serviceId);
        }

        public void Update(ServiceInfo serviceInfo)
        {
            m_allServices = m_allServices
                .Where(p => !p.Id.Equals(serviceInfo.Id))
                .Append(serviceInfo);
            if (serviceInfo.IsLocal)
            {
                m_localServices = m_localServices
                    .Where(p => !p.Id.Equals(serviceInfo.Id))
                    .Append(serviceInfo);
            }

            OnUpdate?.Invoke(this, serviceInfo);
        }

        public event EventHandler<ServiceInfo> OnUpdate;
    }
}