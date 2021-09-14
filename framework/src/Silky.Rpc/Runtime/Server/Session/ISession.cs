namespace Silky.Rpc.Runtime.Server
{
    public interface ISession
    {
        long? UserId { get; }
        
        string UserName { get; }
        
        long?  TenantId { get; }
    }
}