using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transaction;
using Lms.Transaction.Tcc.Executor;

namespace Lms.Transaction.Tcc.Handlers
{
    public class ConsumerTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ILmsMethodInvocation invocation)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];
            if (serviceEntry.IsLocal)
            {
                try
                {
                    await executor.ConsumerParticipantExecute(serviceEntry, serviceKey, parameters, TccMethodType.Try);
                    context.Action = ActionStage.Confirming;
                    invocation.ReturnValue = await executor.ConsumerParticipantExecute(serviceEntry, serviceKey,
                        parameters, TccMethodType.Confirm);
                }
                catch (Exception ex)
                {
                    await executor.ConsumerParticipantExecute(serviceEntry, serviceKey, parameters,
                        TccMethodType.Cancel);
                    throw ex;
                }
            }
            else
            {
                context.TransactionRole = TransactionRole.Consumer;
                context.Action = ActionStage.Trying;
                await invocation.ProceedAsync();
            }
        }
    }
}