namespace Lms.Core.Exceptions
{
    public class NotFindServiceRouteAddressException: LmsException
    {
        public NotFindServiceRouteAddressException(string message) : base(message, StatusCode.NotFindServiceRouteAddress)
        {
        }
    }
}