using Silky.Core.DependencyInjection;
using Silky.Rpc.Address.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Routing.Descriptor
{
    public interface IRouteDescriptorProvider : ITransientDependency
    {
        RouteDescriptor Create(AddressDescriptor addressDescriptor, ServiceProtocol serviceProtocol);
    }
}