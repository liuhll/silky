namespace Silky.Lms.Core.Exceptions
{
    public class RpcAuthenticationException : LmsException
    {
        public RpcAuthenticationException(string message) : base(message, StatusCode.RpcUnAuthentication)
        {

        }
    }
}