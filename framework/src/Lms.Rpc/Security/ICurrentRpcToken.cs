namespace Lms.Rpc.Security
{
    public interface ICurrentRpcToken
    {
        string Token { get; }
    }
}