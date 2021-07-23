using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Handler;
using Silky.Transaction.Tcc.Exceptions;

namespace Silky.Transaction.Tcc.Handlers
{
    public class LocalTccTransactionHandler : ITransactionHandler
    {
        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            Debug.Assert(serviceEntry.IsLocal, "Not a local ServiceEntry");

            switch (context.Action)
            {
                case ActionStage.Confirming:
                    await invocation.ExcuteTccMethod(TccMethodType.Confirm, context);
                    break;
                case ActionStage.Canceling:
                    await invocation.ExcuteTccMethod(TccMethodType.Cancel, context);
                    break;
                default:
                    throw new TccTransactionException("事务参与者状态不正确");
            }
        }
    }
}