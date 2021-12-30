﻿using Silky.Core;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Transport;

namespace Silky.Transaction.Abstraction
{
    public static class RpcContextExtensions
    {
        public static TransactionContext GetTransactionContext(this RpcContext rpcContext)
        {
            var transactionContextAttachment = rpcContext.GetInvokeAttachment(AttachmentKeys.TransactionContext);

            if (transactionContextAttachment == null)
            {
                return null;
            }

            var serializer = EngineContext.Current.Resolve<ISerializer>();
            var transactionContext =
                serializer.Deserialize<TransactionContext>(transactionContextAttachment.ToString());
            return transactionContext;
        }

        public static void SetTransactionContext(this RpcContext rpcContext, TransactionContext transactionContext)
        {
            var serializer = EngineContext.Current.Resolve<ISerializer>();

            rpcContext.SetInvokeAttachment(AttachmentKeys.TransactionContext, serializer.Serialize(transactionContext));
        }

        // public static void RemoveTransactionContext(this RpcContext rpcContext, TransactionContext transactionContext)
        // {
        //     
        // }
    }
}