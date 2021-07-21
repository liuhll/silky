namespace Silky.Rpc.Runtime.Server.Descriptor
{
    public interface IServiceDescriptorProvider
    {
        public abstract void Apply(ServiceDescriptor descriptor);
    }
}