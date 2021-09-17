using System;
using Microsoft.Extensions.Logging;

namespace Silky.Core.Exceptions
{
    public class UserFriendlyException : SilkyException
    {
        public UserFriendlyException(string message) : base(message, Exceptions.StatusCode.UserFriendly)
        {
            LogLevel = LogLevel.Warning;
        }

        public UserFriendlyException(string message, Exception innerException) : base(message, innerException,
            StatusCode.UserFriendly)
        {
            LogLevel = LogLevel.Warning;
        }
    }
}