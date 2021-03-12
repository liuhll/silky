using Lms.Rpc.Transport;

namespace Lms.Rpc.Transaction
{
    public static class RpcContextExtensions
    {
        public static TransactionContext GetTransactionContext(this RpcContext rpcContext)
        {
            var transactionContext =
                RpcContext.GetContext().GetAttachment("transactionContext") as TransactionContext;
            return transactionContext;
        }
    }
}