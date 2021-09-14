namespace Silky.Rpc.Runtime.Server
{
    public class NullSession : SessionBase
    {
        private NullSession()
        {
        }

        public static ISession Instance { get; } = new RpcContextSession();

        public override long? UserId { get; } = Instance.UserId;
        public override string UserName { get; } = Instance.UserName;

        public override long? TenantId { get; } = Instance.TenantId;
    }
}