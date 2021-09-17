using Silky.Core.Exceptions;

namespace Silky.Transaction.Exceptions
{
    public class TransactionException : SilkyException
    {
        public TransactionException(string message) : base(message, Core.Exceptions.StatusCode.BusinessError)
        {
        }
    }
}