using System;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Handler;
using Silky.Transaction.Participant;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class ParticipantTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            IParticipant participant = null;
            switch (context.Action)
            {
                case ActionStage.Trying:
                    try
                    {
                        participant = executor.PreTryParticipant(context, invocation);
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
                            SilkyTransactionHolder.Instance.CurrentTransaction.RemoveParticipant(participant);
                        }
                        throw;
                    }
                    break;
                case ActionStage.Confirming:
                    await invocation.ExcuteTccMethod(TccMethodType.Confirm);
                    break;
                case ActionStage.Canceling:
                    await invocation.ExcuteTccMethod(TccMethodType.Cancel);
                    break;
            }
        }
    }
}