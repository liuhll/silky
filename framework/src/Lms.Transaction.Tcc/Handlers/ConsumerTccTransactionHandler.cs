using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transaction;
using Lms.Rpc.Transport;
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
                context.Action = ActionStage.PreTry;
                await executor.ConsumerParticipantExecute(context, invocation, TccMethodType.Try);
                context.Action = ActionStage.Trying;
                invocation.ReturnValue = await executor.ConsumerParticipantExecute(context, invocation, TccMethodType.Confirm);
                context.Action = ActionStage.Confirming;
            }
            else
            {
                if (context.Action != ActionStage.PreTry)
                {
                    context.TransactionRole = TransactionRole.Participant;
                }

                var participant = executor.PreTryParticipant(context, invocation);
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
                    var currentTrans = LmsTransactionHolder.Instance.CurrentTransaction;
                    foreach (var participantItem in currentTrans.Participants)
                    {
                        if (participantItem.Role == TransactionRole.Participant &&
                            participantItem.Status == ActionStage.Trying)
                        {
                            context.Action = ActionStage.Canceling;
                            context.TransactionRole = TransactionRole.Participant;
                            context.ParticipantId = participantItem.ParticipantId;
                            context.ParticipantRefId = participantItem.ParticipantRefId;
                            RpcContext.GetContext().SetTransactionContext(context);
                            await participantItem.Invocation.ProceedAsync();
                        }
                        
                        if (participantItem.Role == TransactionRole.Consumer &&
                            participantItem.Status == ActionStage.Trying)
                        {
                            invocation.ReturnValue = await executor.ConsumerParticipantExecute(context, invocation, TccMethodType.Cancel);
                            context.Action = ActionStage.Canceling;
                        }
                    }

                    throw;
                }
            }
        }
    }
}