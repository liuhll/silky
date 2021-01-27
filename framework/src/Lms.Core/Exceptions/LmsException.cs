using System;
using System.Runtime.Serialization;

namespace Lms.Core.Exceptions
{
    public class LmsException : Exception
    {
        protected StatusCode _exceptionCode;
        public LmsException()
        {

        }
        
        public LmsException(string message, StatusCode status = StatusCode.PlatformError)
            : base(message)
        {
            _exceptionCode = status;
        }
        
        public LmsException(string message, Exception innerException, StatusCode status = StatusCode.PlatformError)
            : base(message, innerException)
        {
            _exceptionCode = status;
        }
        
        public LmsException(SerializationInfo serializationInfo, StreamingContext context, StatusCode status = StatusCode.PlatformError)
            : base(serializationInfo, context)
        {
            _exceptionCode = status;
        }
        
        public StatusCode ExceptionCode { get { return _exceptionCode; } }
    }
}