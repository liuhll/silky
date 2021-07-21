using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Transaction.Handler;
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
                    var currentTrans = SilkyTransactionHolder.Instance.CurrentTransaction;
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