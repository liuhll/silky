using System;
using System.Runtime.Serialization;

namespace Silky.Core.Exceptions
{
    public class NotFindServiceEntryException : SilkyException
    {
        public NotFindServiceEntryException(string message, StatusCode status = StatusCode.NotFindServiceEntry) : base(
            message, status)
        {
        }

        public NotFindServiceEntryException(string message, Exception innerException,
            StatusCode status = StatusCode.NotFindServiceEntry) : base(message, innerException, status)
        {
        }

        public NotFindServiceEntryException(SerializationInfo serializationInfo, StreamingContext context,
            StatusCode status = StatusCode.NotFindServiceEntry) : base(serializationInfo, context, status)
        {
        }
    }
}