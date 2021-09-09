using Silky.Core.Exceptions;
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceInfoExtensions
    {
        public static ServiceRouteDescriptor CreateLocalRouteDescriptor(this ServiceInfo serviceInfo)
        {
            if (!serviceInfo.IsLocal)
            {
                throw new SilkyException("Only allow local service to generate routing descriptors");
            }

            return new ServiceRouteDescriptor()
            {
                Service = serviceInfo.ServiceDescriptor,
                Addresses = new[]
                    {NetUtil.GetRpcAddressModel().Descriptor},
            };
        }
    }
}