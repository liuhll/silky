using Silky.Transaction.Abstraction;

namespace Silky.Transaction
{
    public interface ISilkyContext
    {
        void Set(TransactionContext context);

        TransactionContext Get();

        void Remove();
    }
}