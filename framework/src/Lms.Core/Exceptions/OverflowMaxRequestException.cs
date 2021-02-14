namespace Lms.Core.Exceptions
{
    public class OverflowMaxRequestException : LmsException
    {
        public OverflowMaxRequestException(string message) : base(message, StatusCode.OverflowMaxRequest)
        {

        }
    }
}