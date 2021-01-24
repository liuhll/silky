namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    public interface IServiceBundleProvider
    { 
        string RouteTemplate { get; }

        bool IsPrefix { get; }
        
    }
}