using System;
using System.Runtime.Serialization;

namespace Lms.Core.Exceptions
{
    public class LmsException : Exception
    {
        public LmsException()
        {

        }
        
        public LmsException(string message)
            : base(message)
        {

        }
        
        public LmsException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
        
        public LmsException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }
    }
}