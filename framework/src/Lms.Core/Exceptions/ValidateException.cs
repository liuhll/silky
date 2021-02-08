namespace Lms.Core.Exceptions
{
    public class ValidateException : LmsException
    {
        public ValidateException(string message) : base(message, StatusCode.ValidateError)
        {

        }
    }
}