namespace Silky.Rpc.Runtime.Server
{
    public interface ISession
    {
        object UserId { get; }

        string UserName { get; }

        object TenantId { get; }
    }
}