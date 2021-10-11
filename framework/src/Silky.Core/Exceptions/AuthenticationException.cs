namespace Silky.Core.Exceptions
{
    public class AuthenticationException : SilkyException
    {
        public AuthenticationException(string message) : base(message, StatusCode.UnAuthentication)
        {
        }
    }
}