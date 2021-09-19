using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Silky.Core.Logging;

namespace Silky.Core.Exceptions
{
    public class SilkyException : Exception, IHasErrorCode, IHasLogLevel
    {
        protected StatusCode _statusCode;


        public SilkyException(string message, StatusCode status = StatusCode.FrameworkException)
            : base(message)
        {
            _statusCode = status;
            LogLevel = LogLevel.Error;
        }

        public SilkyException(string message, Exception innerException,
            StatusCode status = StatusCode.FrameworkException)
            : base(message, innerException)
        {
            _statusCode = status;
            LogLevel = LogLevel.Error;
        }

        public SilkyException(SerializationInfo serializationInfo, StreamingContext context,
            StatusCode status = StatusCode.FrameworkException)
            : base(serializationInfo, context)
        {
            _statusCode = status;
            LogLevel = LogLevel.Error;
        }

        public StatusCode StatusCode
        {
            get { return _statusCode; }
        }

        public LogLevel LogLevel { get; set; }
    }
}