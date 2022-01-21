using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Silky.Core.Logging;

namespace Silky.Core.Exceptions
{
    public class SilkyException : Exception, IHasErrorCode, IHasLogLevel
    {
        protected StatusCode _statusCode;

        protected int? _status;

        public SilkyException(string message, StatusCode statusCode = StatusCode.FrameworkException, int? status = null)
            : base(message)
        {
            _statusCode = statusCode;
            _status = status;
            LogLevel = LogLevel.Error;
        }

        public SilkyException(string message, Exception innerException,
            StatusCode statusCode = StatusCode.FrameworkException, int? status = null)
            : base(message, innerException)
        {
            _statusCode = statusCode;
            _status = status;
            LogLevel = LogLevel.Error;
        }

        public SilkyException(SerializationInfo serializationInfo, StreamingContext context,
            StatusCode statusCode = StatusCode.FrameworkException, int? status = null)
            : base(serializationInfo, context)
        {
            _statusCode = statusCode;
            _status = status;
            LogLevel = LogLevel.Error;
        }

        public StatusCode StatusCode => _statusCode;

        public int Status
        {
            get
            {
                if (_status.HasValue)
                {
                    return _status.Value;
                }

                return (int)_statusCode;
            }
        }

        public LogLevel LogLevel { get; set; }
    }
}