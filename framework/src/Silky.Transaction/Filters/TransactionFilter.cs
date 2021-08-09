using Silky.Core.Rpc;
using Silky.Core.Utils;
using Silky.Rpc.Runtime.Filters;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Filters
{
    public class TransactionFilter : IClientFilter
    {
        public int Order { get; } = 1;

        private TransactionContext context;
        private string participantId;
        private IParticipant participant;

        public void OnActionExecuting(ServiceEntryExecutingContext serviceEntryExecutingContext)
        {
            context = SilkyTransactionContextHolder.Instance.Get();
            if (context == null) return;

            if (!serviceEntryExecutingContext.ServiceEntry.IsTransactionServiceEntry()) return;

            participantId = context.ParticipantId;
            participant = BuildParticipant(context, serviceEntryExecutingContext.ServiceEntry.Id,
                serviceEntryExecutingContext.ServiceKey,
                serviceEntryExecutingContext.Parameters);

            if (participant != null)
            {
                var rpcTransactionContext = new TransactionContext()
                {
                    Action = context.Action,
                    ParticipantId = participant.ParticipantId,
                    ParticipantRefId = context.ParticipantRefId,
                    TransactionRole = context.TransactionRole,
                    TransId = context.TransId,
                    TransType = context.TransType,
                };
                if (context.TransactionRole == TransactionRole.Participant)
                {
                    rpcTransactionContext.ParticipantRefId = context.ParticipantId;
                }

                RpcContext.Context.SetTransactionContext(rpcTransactionContext);
            }
        }

        public void OnActionExecuted(ServiceEntryExecutedContext serviceEntryExecutingContext)
        {
            if (context == null) return;

            if (context.TransactionRole == TransactionRole.Participant)
            {
                SilkyTransactionHolder.Instance.RegisterParticipantByNested(participantId, participant);
            }
            else
            {
                SilkyTransactionHolder.Instance.RegisterStarterParticipant(participant);
            }
        }

        private IParticipant BuildParticipant(TransactionContext context, string serviceId, string serviceKey,
            object[] parameters)
        {
            if (context.Action != ActionStage.Trying)
            {
                return null;
            }

            return new SilkyParticipant()
            {
                ParticipantId = GuidGenerator.CreateGuidStrWithNoUnderline(),
                TransId = context.TransId,
                TransType = context.TransType,
                ServiceId = serviceId,
                ServiceKey = serviceKey,
                Parameters = parameters
                // Status = context.Action
            };
        }
    }
}