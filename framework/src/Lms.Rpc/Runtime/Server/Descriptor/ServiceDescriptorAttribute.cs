using System;

namespace Lms.Rpc.Runtime.Server.Descriptor
{
    public abstract class ServiceDescriptorAttribute : Attribute, IServiceDescriptorProvider
    {
        public abstract void Apply(ServiceDescriptor descriptor);
    }
}