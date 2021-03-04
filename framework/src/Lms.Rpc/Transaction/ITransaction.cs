namespace Lms.Rpc.Transaction
{
    public interface ITransaction
    {
        void FireBeforeTransactionCompletion();

        void FireBeforeTransactionCompletionQuietly();

        void FireAfterTransactionCompletion();

        TransactionStatus GetTransactionStatus();

        void Resume();

        void Suspend();

        bool IsTiming();
        
    }
}