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
            IParticipant participant = null;
            switch (context.Action)
            {
                case ActionStage.Trying:
                    participant = executor.PreTryParticipant(context, invocation);
                    await invocation.ProceedAsync();
                    if (participant != null)
                    {
                        participant.Status = ActionStage.Trying;
                    }
                    break;
                case ActionStage.Confirming:
                    break;
            }
        }
    }
}