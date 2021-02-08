using System;

namespace Lms.Core.Exceptions
{
    public static class ExceptionExtensions
    {
        public static string GetExceptionMessage(this Exception exception)
        {
            var message = exception.Message;
            if (!exception.IsBusinessException())
            {
                message += Environment.NewLine + " 堆栈信息:" + Environment.NewLine + exception.StackTrace;
                if (exception.InnerException != null)
                {
                    message += "|InnerException:" + GetExceptionMessage(exception.InnerException);
                }
            }
            return message;
        }
        
        public static bool IsBusinessException(this Exception exception)
        {
            var statusCode = exception.GetExceptionStatusCode();
            return statusCode == StatusCode.Success
                   || statusCode == StatusCode.ValidateError
                   || statusCode == StatusCode.BusinessError;

        }
        
        public static StatusCode GetExceptionStatusCode(this Exception exception)
        {
            var statusCode = StatusCode.UnPlatformError;
            
            if (exception is LmsException)
            {
                statusCode = ((LmsException)exception).ExceptionCode;
                return statusCode;
            }
            if (exception.InnerException != null)
            {
                return exception.InnerException.GetExceptionStatusCode();
            }
            return statusCode;

        }
    }
}