namespace Silky.Rpc.Security
{
    public interface ICurrentRpcToken
    {
        string Token { get; }
        void SetRpcToken();
    }
}