using Silky.Transaction.Repository.Spi;

namespace Silky.Transaction
{
    public interface ISilkyContext
    {
        void Set(TransactionContext context);

        TransactionContext Get();

        void Remove();
    }
}