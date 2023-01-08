namespace Silky.Rpc.Runtime.Server;

public class ServiceEntryContext
{
    protected ServiceEntryContext(ServiceEntryContext context)
    {
        ServiceEntry = context.ServiceEntry;
        Parameters = context.Parameters;
        ServiceKey = context.ServiceKey;
        ServiceInstance = context.ServiceInstance;
    }
    
    public ServiceEntryContext(ServiceEntry serviceEntry, object[] parameters, string? serviceKey, object instance)
    {
        ServiceEntry = serviceEntry;
        Parameters = parameters;
        ServiceKey = serviceKey;
        ServiceInstance = instance;
    }

    public object ServiceInstance { get; set; }

    public ServiceEntry ServiceEntry { get; set; }
    
    public object[] Parameters { get; set; }

    public string? ServiceKey { get; set; }
}