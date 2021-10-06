namespace Silky.Rpc.Runtime.Server
{
    public class NullSession : SessionBase
    {
        private NullSession()
        {
        }

        public static ISession Instance { get; } = new RpcContextSession();

        public override object? UserId { get; } = Instance.UserId;
        public override string UserName { get; } = Instance.UserName;

        public override object? TenantId { get; } = Instance.TenantId;
    }
}