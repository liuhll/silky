using System.Threading.Tasks;
using Silky.Lms.Rpc.Transport;
using Silky.Lms.Transaction.Participant;

namespace Silky.Lms.Transaction.Tcc
{
    public static class ParticipantExtensions
    {
        public static async Task ParticipantConfirm(this IParticipant participant)
        {
            var invocation = participant.Invocation;
            if (participant.ParticipantType == ParticipantType.Local
                && (participant.Role == TransactionRole.Start || participant.Role == TransactionRole.Consumer))
            {
                await invocation.ExcuteTccMethod(TccMethodType.Confirm);
            }
            else
            {
                var context = RpcContext.GetContext().GetTransactionContext();
                context.TransactionRole = TransactionRole.Participant;
                context.Action = ActionStage.Confirming;
                await invocation.ProceedAsync();
            }
        }

        public static async Task ParticipantCancel(this IParticipant participant)
        {
            var invocation = participant.Invocation;
            if (participant.ParticipantType == ParticipantType.Local
                && (participant.Role == TransactionRole.Start || participant.Role == TransactionRole.Consumer))
            {
                await invocation.ExcuteTccMethod(TccMethodType.Cancel);
            }
            else
            {
                var context = RpcContext.GetContext().GetTransactionContext();
                context.TransactionRole = TransactionRole.Participant;
                context.Action = ActionStage.Canceling;
                await invocation.ProceedAsync();
            }
        }
    }
}