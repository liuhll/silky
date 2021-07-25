using System.Threading;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction
{
    public class ThreadLocalSilkyContext : ISilkyContext
    {
        private static AsyncLocal<TransactionContext> CURRENT_LOCAL = new();


        public void Set(TransactionContext context)
        {
            CURRENT_LOCAL.Value = context;
        }

        public TransactionContext Get()
        {
            return CURRENT_LOCAL.Value;
        }

        public void Remove()
        {
            CURRENT_LOCAL.Value = null;
        }
    }
}