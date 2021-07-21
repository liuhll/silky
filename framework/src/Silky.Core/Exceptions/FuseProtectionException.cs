using System;

namespace Silky.Core.Exceptions
{
    public class FuseProtectionException : SilkyException
    {
        public FuseProtectionException(string message) : base(message, StatusCode.FuseProtection)
        {
        }

        public FuseProtectionException(string message, Exception innerException) : base(message, innerException,
            StatusCode.FuseProtection)
        {
        }
    }
}