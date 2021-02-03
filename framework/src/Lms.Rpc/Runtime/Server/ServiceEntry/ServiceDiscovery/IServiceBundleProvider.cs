namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    public interface IServiceBundleProvider
    { 
        string Template { get; }
        
        ServiceProtocol ServiceProtocol { get; }
    }
}