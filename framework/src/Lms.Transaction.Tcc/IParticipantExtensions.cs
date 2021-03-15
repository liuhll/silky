using System.Threading.Tasks;
using Lms.Rpc.Transaction;

namespace Lms.Transaction.Tcc
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
                await invocation.ProceedAsync();
            }
        }
    }
}