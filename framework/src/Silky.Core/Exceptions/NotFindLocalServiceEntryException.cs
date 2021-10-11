namespace Silky.Core.Exceptions
{
    public class NotFindLocalServiceEntryException : SilkyException
    {
        public NotFindLocalServiceEntryException(string message) : base(message, StatusCode.NotFindLocalServiceEntry)
        {
        }
    }
}