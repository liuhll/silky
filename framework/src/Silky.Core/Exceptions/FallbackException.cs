using System;

namespace Silky.Core.Exceptions
{
    public class FallbackException : SilkyException, INotNeedFallback
    {
        public FallbackException(string message) : base(message, StatusCode.FallbackExecFail)
        {
        }

        public FallbackException(string message, Exception innerException) : base(message, innerException,
            StatusCode.FallbackExecFail)

        {
        }
    }
}