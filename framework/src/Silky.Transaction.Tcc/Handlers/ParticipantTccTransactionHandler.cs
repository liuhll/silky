using System;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Transport;
using Silky.Transaction.Handler;
using Silky.Transaction.Participant;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class ParticipantTccTransactionHandler : ITransactionHandler
    {
        private readonly TccTransactionExecutor _executor = TccTransactionExecutor.Executor;

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            switch (context.Action)
            {
                case ActionStage.PreTry:
                    await invocation.ExcuteTccMethod(TccMethodType.Try, context);

                    break;
                case ActionStage.Trying:
                    await invocation.ExcuteTccMethod(TccMethodType.Confirm, context);
                    break;
                case ActionStage.Canceling:
                    await invocation.ExcuteTccMethod(TccMethodType.Cancel, context);
                    break;
            }
        }
    }
}