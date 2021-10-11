namespace Silky.Core.Exceptions
{
    public class BusinessException : SilkyException
    {
        public BusinessException(string message) : base(message, StatusCode.BusinessError)
        {
        }
    }
}