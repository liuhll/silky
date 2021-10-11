namespace Silky.Core.Exceptions
{
    public class AuthorizationException : SilkyException
    {
        public AuthorizationException(string message) : base(message, StatusCode.UnAuthorization)
        {
        }
    }
}