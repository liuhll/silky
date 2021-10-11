namespace Silky.Core.Exceptions
{
    public class NotFindServiceRouteAddressException : SilkyException
    {
        public NotFindServiceRouteAddressException(string message) : base(message,
            StatusCode.NotFindServiceRouteAddress)
        {
        }
    }
}