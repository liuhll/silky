using System.Collections.Generic;
using Silky.Rpc.CachingInterceptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetServiceEntryDetailOutput : GetServiceEntryOutput
    {
        public ServiceEntryGovernance Governance { get; set; }

        public ICollection<ServiceEntryCacheTemplateOutput> CacheTemplates { get; set; }

        public ICollection<ServiceKeyOutput> ServiceKeys { get; set; }

        public FallbackOutput Fallback { get; set; }
        public bool SupportCachingIntercept => Governance.EnableCachingInterceptor && CacheTemplates?.Count > 0;
    }

    public class FallbackOutput
    {
        public string TypeName { get; set; }

        public string MethodName { get; set; }

    }

    public class ServiceKeyOutput
    {
        public string Name { get; set; }

        public int Weight { get; set; }
    }

    public class ServiceEntryCacheTemplateOutput
    {
        public string KeyTemplete { get; set; }

        public bool OnlyCurrentUserData { get; set; }

        public CachingMethod CachingMethod { get; set; }
    }
}