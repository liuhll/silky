namespace Lms.Rpc.Runtime.Session
{
    public abstract class SessionBase : ISession
    {
        public abstract long? UserId { get; }
        public abstract string UserName { get; }
        public abstract long? TenantId { get; }
    }
}