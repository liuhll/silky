namespace Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor
{
    public interface IServiceDescriptorProvider
    {
        public abstract void Apply(ServiceDescriptor descriptor);
    }
}