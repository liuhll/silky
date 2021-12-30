namespace Silky.Core.Runtime.Session
{
    public interface ISession
    {
        object UserId { get; }

        string UserName { get; }

        object TenantId { get; }
    }
}