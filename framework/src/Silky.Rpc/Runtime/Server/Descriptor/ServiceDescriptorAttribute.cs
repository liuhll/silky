using System;

namespace Silky.Rpc.Runtime.Server.Descriptor
{
    public abstract class ServiceDescriptorAttribute : Attribute, IServiceDescriptorProvider
    {
        public abstract void Apply(ServiceDescriptor descriptor);
    }
}