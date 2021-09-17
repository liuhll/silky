namespace Silky.Core.Exceptions
{
    public class OverflowMaxRequestException : SilkyException
    {
        public OverflowMaxRequestException(string message) : base(message, StatusCode.OverflowMaxRequest)
        {
        }
    }
}