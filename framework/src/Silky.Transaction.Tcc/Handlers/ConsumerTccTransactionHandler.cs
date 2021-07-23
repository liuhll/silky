using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Transaction.Handler;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Tcc.Exceptions;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class ConsumerTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);

            if (serviceEntry.IsLocal)
            {
                switch (context.Action)
                {
                    case ActionStage.PreTry:
                        executor.PreTryParticipant(context, invocation);
                        try
                        {
                            await invocation.ProceedAsync();
                            context.Action = ActionStage.Trying;
                            var currentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                            currentTransaction.Status = context.Action;
                            await executor.GlobalConfirm(currentTransaction);
                        }
                        catch (Exception e)
                        {
                            var currentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                            await executor.GlobalCancel(currentTransaction);
                            throw;
                        }

                        break;
                  
                    default:
                        throw new TccTransactionException("事务参与者状态不正确");
                }
            }
            else
            {
                var participant = executor.PreTryParticipant(context, invocation);
                if (participant != null)
                {
                    context.TransactionRole = TransactionRole.Consumer;
                    RpcContext.GetContext().SetTransactionContext(context);
                }

                try
                {
                    await invocation.ProceedAsync();
                    if (participant != null)
                    {
                        participant.Status = ActionStage.Trying;
                    }
                }
                catch (Exception e)
                {
                    if (participant != null)
                    {
                        participant.Status = ActionStage.Error;
                    }

                    throw;
                }
            }
        }
    }
}