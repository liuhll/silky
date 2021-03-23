using System;

namespace Lms.Core.Exceptions
{
    public class FuseProtectionException : LmsException
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