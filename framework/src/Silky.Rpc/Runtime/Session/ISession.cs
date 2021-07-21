namespace Silky.Rpc.Runtime.Session
{
    public interface ISession
    {
        long? UserId { get; }
        
        string UserName { get; }
        
        long?  TenantId { get; }
    }
}