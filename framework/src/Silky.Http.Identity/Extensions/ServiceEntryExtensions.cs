using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Identity.Extensions
{
    public static class ServiceEntryExtensions
    {
        public static bool IsSilkyAppService(this ServiceEntry serviceEntry)
        {
            return "Silky.Http.Dashboard.AppService.ISilkyAppService".Equals(serviceEntry.ServiceType.FullName);
        }
    }
}