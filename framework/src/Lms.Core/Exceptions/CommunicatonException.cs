using System;

namespace Lms.Core.Exceptions
{
    public class CommunicatonException : LmsException
    {
        public CommunicatonException(string message) : base(message, StatusCode.CommunicatonError)
        {
        }

        public CommunicatonException(string message, Exception innerException) : base(message, innerException,
            StatusCode.CommunicatonError)
        {
        }
    }
}