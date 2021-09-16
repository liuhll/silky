namespace Silky.Core.Exceptions
{
    public class OverflowMaxRequestException : SilkyException, INotNeedFallback
    {
        public OverflowMaxRequestException(string message) : base(message, StatusCode.OverflowMaxRequest)
        {
        }
    }
}