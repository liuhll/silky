using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Extensions;
using Silky.Core.Serialization;
using Silky.Rpc.Transport;

namespace Silky.Transaction
{
    public static class RpcContextExtensions
    {
        public static TransactionContext GetTransactionContext(this RpcContext rpcContext)
        {
            var transactionContextAttachment = rpcContext.GetAttachment(AttachmentKeys.TransactionContext);

            if (transactionContextAttachment == null)
            {
                return null;
            }

            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var transactionContext = serializer.Deserialize<TransactionContext>(transactionContextAttachment.ToString());
            return transactionContext;
        }

        public static void SetTransactionContext(this RpcContext rpcContext, TransactionContext transactionContext)
        {
            var serializer = EngineContext.Current.Resolve<ISerializer>();

            rpcContext.SetAttachment(AttachmentKeys.TransactionContext, serializer.Serialize(transactionContext));
        }
    }
}