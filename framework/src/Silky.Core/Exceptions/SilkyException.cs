using System;
using System.Runtime.Serialization;

namespace Silky.Core.Exceptions
{
    public class SilkyException : Exception, IHasErrorCode
    {
        protected StatusCode _exceptionCode;

        public SilkyException()
        {
        }

        public SilkyException(string message, StatusCode status = StatusCode.PlatformError)
            : base(message)
        {
            _exceptionCode = status;
        }

        public SilkyException(string message, Exception innerException, StatusCode status = StatusCode.PlatformError)
            : base(message, innerException)
        {
            _exceptionCode = status;
        }

        public SilkyException(SerializationInfo serializationInfo, StreamingContext context,
            StatusCode status = StatusCode.PlatformError)
            : base(serializationInfo, context)
        {
            _exceptionCode = status;
        }

        public StatusCode ExceptionCode
        {
            get { return _exceptionCode; }
        }
    }
}