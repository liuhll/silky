using System;
using System.Runtime.Serialization;

namespace Silky.Core.Exceptions
{
    public class CommunicationException : SilkyException
    {
        public CommunicationException(string message, StatusCode status = StatusCode.CommunicatonError) : base(message,
            status)
        {
        }

        public CommunicationException(string message, Exception innerException,
            StatusCode status = StatusCode.CommunicatonError) : base(message, innerException, status)
        {
        }

        public CommunicationException(SerializationInfo serializationInfo, StreamingContext context,
            StatusCode status = StatusCode.CommunicatonError) : base(serializationInfo, context, status)
        {
        }
    }
}