namespace Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery
{
    public interface IServiceBundleProvider
    { 
        string Template { get; }

        bool IsPrefix { get; }
        
    }
}