namespace Silky.Core.Runtime.Session
{
    public abstract class SessionBase : ISession
    {
        public abstract object? UserId { get; }
        public abstract string UserName { get; }
        public abstract object? TenantId { get; }
    }
}