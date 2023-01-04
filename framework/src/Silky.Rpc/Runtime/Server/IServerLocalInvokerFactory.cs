namespace Silky.Rpc.Runtime.Server;

public interface IServerLocalInvokerFactory
{
    LocalInvoker CreateInvoker(ServiceEntryContext serviceEntryContext);  
}