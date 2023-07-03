using System;
using System.Runtime.Serialization;

namespace Silky.Core.Exceptions
{
    public class CommunicationException : SilkyException
    {
        public CommunicationException(string message, StatusCode status = StatusCode.CommunicationError) : base(message,
            status)
        {
        }

        public CommunicationException(string message, Exception innerException,
            StatusCode status = StatusCode.CommunicationError) : base(message, innerException, status)
        {
        }

        public CommunicationException(SerializationInfo serializationInfo, StreamingContext context,
            StatusCode status = StatusCode.CommunicationError) : base(serializationInfo, context, status)
        {
        }
    }
}