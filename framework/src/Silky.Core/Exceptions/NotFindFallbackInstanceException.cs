namespace Silky.Core.Exceptions
{
    public class NotFindFallbackInstanceException : SilkyException, INotNeedFallback
    {
        public NotFindFallbackInstanceException(string message) : base(message, StatusCode.NotFindFallbackInstance)
        {
        }
    }
}