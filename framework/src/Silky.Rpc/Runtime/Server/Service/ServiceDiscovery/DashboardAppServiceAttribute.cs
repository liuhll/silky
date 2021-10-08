using System;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class DashboardAppServiceAttribute : MetadataAttribute
    {
        public DashboardAppServiceAttribute() : base(ServiceConstant.IsSilkyDashboardService, true)
        {
        }
    }
}