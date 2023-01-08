namespace Silky.Rpc.Runtime.Server;

public interface IServerLocalInvokerFactory
{
    ILocalInvoker CreateInvoker(ServiceEntryContext serviceEntryContext);  
}