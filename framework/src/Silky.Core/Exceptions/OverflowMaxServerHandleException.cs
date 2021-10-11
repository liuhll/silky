namespace Silky.Core.Exceptions
{
    public class OverflowMaxServerHandleException : SilkyException
    {
        public OverflowMaxServerHandleException(string message) : base(message, StatusCode.OverflowMaxServerHandle)
        {
        }
    }
}