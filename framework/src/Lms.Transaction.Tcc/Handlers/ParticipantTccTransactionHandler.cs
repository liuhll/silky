using System;
using System.Threading.Tasks;
using Lms.Core.DynamicProxy;
using Lms.Rpc.Transaction;
using Lms.Transaction.Tcc.Executor;

namespace Lms.Transaction.Tcc.Handlers
{
    public class ParticipantTccTransactionHandler : ITransactionHandler
    {
        private TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ILmsMethodInvocation invocation)
        {
            switch (context.Action)
            {
                case ActionStage.Trying:
                    var participant = executor.PreTryParticipant(context, invocation);
                    await invocation.ProceedAsync();
                    if (participant != null)
                    {
                        participant.Status = ActionStage.Trying;
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