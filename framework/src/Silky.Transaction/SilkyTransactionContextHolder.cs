using System.Threading;
using Silky.Core;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction
{
    public class SilkyTransactionContextHolder
    {
        private static readonly AsyncLocal<TransactionContext> CURRENT_LOCAL = new();

        private SilkyTransactionContextHolder()
        {
        }

        public static SilkyTransactionContextHolder Instance => Singleton<SilkyTransactionContextHolder>.Instance ??
                                                                (Singleton<SilkyTransactionContextHolder>.Instance =
                                                                    new SilkyTransactionContextHolder());

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