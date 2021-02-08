namespace Lms.Core.Exceptions
{
    public class BusinessException : LmsException
    {
        public BusinessException(string message) : base(message, StatusCode.BusinessError)
        {

        }
    }
}