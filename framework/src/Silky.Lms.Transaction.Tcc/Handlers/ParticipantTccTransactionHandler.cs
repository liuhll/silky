using System;
using System.Threading.Tasks;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.Transaction.Handler;
using Silky.Lms.Transaction.Participant;
using Silky.Lms.Transaction.Tcc.Executor;

namespace Silky.Lms.Transaction.Tcc.Handlers
{
    public class ParticipantTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ILmsMethodInvocation invocation)
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
                            LmsTransactionHolder.Instance.CurrentTransaction.RemoveParticipant(participant);
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