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

            if (serviceEntry.IsLocal)
            {
                try
                {
                    context.Action = ActionStage.PreTry;
                    await executor.ConsumerParticipantExecute(context, invocation, TccMethodType.Try);

                    context.Action = ActionStage.Trying;
                    var currentTrans = LmsTransactionHolder.Instance.CurrentTransaction;
                    if (currentTrans != null)
                    {
                        foreach (var participant in currentTrans.Participants)
                        {
                            if (participant.Role == TransactionRole.Participant)
                            {
                                await participant.ParticipantConfirm();
                            }
                        }
                    }

                    invocation.ReturnValue =
                        await executor.ConsumerParticipantExecute(context, invocation, TccMethodType.Confirm);

                    context.Action = ActionStage.Confirming;
                }
                catch (Exception)
                {
                    var currentTrans = LmsTransactionHolder.Instance.CurrentTransaction;
                    if (currentTrans != null)
                    {
                        foreach (var participant in currentTrans.Participants)
                        {
                            if (participant.Role == TransactionRole.Participant)
                            {
                                await participant.ParticipantConfirm();
                            }
                        }
                    }

                    invocation.ReturnValue =
                        await executor.ConsumerParticipantExecute(context, invocation, TccMethodType.Cancel);


                    throw;
                }
            }
            else
            {
                if (context.Action != ActionStage.PreTry)
                {
                    context.TransactionRole = TransactionRole.Participant;
                }

                await invocation.ProceedAsync();
            }
        }
    }
}