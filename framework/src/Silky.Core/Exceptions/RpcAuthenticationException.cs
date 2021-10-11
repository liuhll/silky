namespace Silky.Core.Exceptions
{
    public class RpcAuthenticationException : SilkyException
    {
        public RpcAuthenticationException(string message) : base(message, StatusCode.RpcUnAuthentication)
        {
        }
    }
}