namespace Lms.Rpc.Transaction
{
    public interface ITransactionHandlerFactory
    {
        ITransactionHandler FactoryOf(TransactionContext context);
    }
}