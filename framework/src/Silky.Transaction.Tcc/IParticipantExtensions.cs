using System.Threading.Tasks;
using Silky.Rpc.Transport;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Tcc
{
    public static class ParticipantExtensions
    {
        public static async Task ParticipantConfirm(this IParticipant participant)
        {
            var invocation = participant.Invocation;
            var context = RpcContext.GetContext().GetTransactionContext();

            // 只有内部调用的才需要提交
            if (participant.ParticipantType == ParticipantType.Inline)
            {
                context.Action = ActionStage.Confirming;
                context.TransactionRole = TransactionRole.Local;
                RpcContext.GetContext().SetTransactionContext(context);
                await invocation.ProceedAsync();
            }
            else
            {
                if (participant.Role == TransactionRole.Start)
                {
                    await invocation.ExcuteTccMethod(TccMethodType.Confirm, context);
                }
            }
        }

        public static async Task ParticipantCancel(this IParticipant participant)
        {
            var invocation = participant.Invocation;
            var context = RpcContext.GetContext().GetTransactionContext();
            if (participant.ParticipantType == ParticipantType.Inline)
            {
                if (participant.Status == ActionStage.Trying)
                {
                    context.Action = ActionStage.Canceling;
                    context.TransactionRole = TransactionRole.Local;
                    RpcContext.GetContext().SetTransactionContext(context);
                    await invocation.ProceedAsync();
                }
            }
            else
            {
                if (participant.Role == TransactionRole.Start && participant.Status == ActionStage.Trying)
                {
                    await invocation.ExcuteTccMethod(TccMethodType.Cancel, context);
                }
            }
        }
    }
}