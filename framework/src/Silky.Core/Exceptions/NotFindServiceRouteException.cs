using System;
using System.Runtime.Serialization;

namespace Silky.Core.Exceptions
{
    public class NotFindServiceRouteException : SilkyException
    {
        public NotFindServiceRouteException(string message, StatusCode status = StatusCode.NotFindServiceRoute) : base(
            message, status)
        {
        }

        public NotFindServiceRouteException(string message, Exception innerException,
            StatusCode status = StatusCode.NotFindServiceRoute) : base(message, innerException, status)
        {
        }

        public NotFindServiceRouteException(SerializationInfo serializationInfo, StreamingContext context,
            StatusCode status = StatusCode.NotFindServiceRoute) : base(serializationInfo, context, status)
        {
        }
    }
}