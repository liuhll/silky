using System.Diagnostics;
using System.Threading.Tasks;
using Silky.Core.DynamicProxy;
using Silky.Core.Extensions;
using Silky.Core.Utils;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Transport;
using Silky.Transaction.Participant;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Silky.Transaction.Tcc.Executor
{
    public sealed class TccTransactionExecutor
    {
        private static TccTransactionExecutor executor = new();
        public ILogger<TccTransactionExecutor> Logger { get; set; }

        private TccTransactionExecutor()
        {
            Logger = NullLogger<TccTransactionExecutor>.Instance;
        }

        public static TccTransactionExecutor Executor => executor;


        public ITransaction PreTry(ISilkyMethodInvocation invocation)
        {
            Logger.LogDebug("tcc transaction starter");
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            Debug.Assert(serviceEntry.IsLocal, "Not a local ServiceEntry");
            var participantType = serviceEntry.IsLocal ? ParticipantType.Local : ParticipantType.Inline;

            var transaction = CreateTransaction();
            var participant = BuildParticipant(invocation,
                null,
                null,
                TransactionRole.Start,
                participantType,
                transaction.TransId);
            transaction.RegisterParticipant(participant);
            SilkyTransactionHolder.Instance.Set(transaction);
            var context = new TransactionContext
            {
                Action = ActionStage.PreTry,
                TransId = transaction.TransId,
                TransactionRole = TransactionRole.Start,
                TransType = TransactionType.Tcc
            };
            RpcContext.GetContext().SetTransactionContext(context);
            return transaction;
        }

        public IParticipant PreTryParticipant(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            Logger.LogDebug($"participant tcc transaction start..：{context}");
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            if (serviceEntry.IsTransactionServiceEntry())
            {
                var participantType = serviceEntry.IsLocal ? ParticipantType.Local : ParticipantType.Inline;
                var participantRole = serviceEntry.IsLocal ? TransactionRole.Consumer : TransactionRole.Participant;
                var participant = BuildParticipant(invocation,
                    null,
                    context.ParticipantId,
                    participantRole,
                    participantType,
                    context.TransId);
                participant.Status = ActionStage.PreTry;
                if (SilkyTransactionHolder.Instance.CurrentTransaction != null)
                {
                    SilkyTransactionHolder.Instance.CurrentTransaction.RegisterParticipant(participant);
                }
                else
                {
                    var transaction = CreateTransaction(context.TransId);
                    participant.Role = TransactionRole.Consumer;
                    transaction.RegisterParticipant(participant);
                    SilkyTransactionHolder.Instance.Set(transaction);
                }

                return participant;
            }

            return null;
        }

        public async Task GlobalConfirm(ITransaction transaction)
        {
            foreach (var participant in transaction.Participants)
            {
                await participant.ParticipantConfirm();
            }

            if (SilkyTransactionHolder.Instance.CurrentTransaction != null)
            {
                SilkyTransactionHolder.Instance.CurrentTransaction.Status = ActionStage.Canceled;
            }
        }

        public async Task GlobalCancel(ITransaction transaction)
        {
            foreach (var participant in transaction.Participants)
            {
                await participant.ParticipantCancel();

                transaction.Status = ActionStage.Canceled;
            }

            if (SilkyTransactionHolder.Instance.CurrentTransaction != null)
            {
                SilkyTransactionHolder.Instance.CurrentTransaction.Status = ActionStage.Canceled;
            }
        }

        private IParticipant BuildParticipant(ISilkyMethodInvocation invocation,
            string participantId,
            string participantRefId,
            TransactionRole transactionRole,
            ParticipantType participantType,
            string transId)
        {
            var participant = new SilkyParticipant()
            {
                Role = transactionRole,
                TransId = transId,
                TransType = TransactionType.Tcc,
                ParticipantType = participantType,
                Invocation = invocation,
            };
            if (participantId.IsNullOrEmpty())
            {
                participant.ParticipantId = GuidGenerator.CreateGuidStrWithNoUnderline();
            }
            else
            {
                participant.ParticipantId = participantId;
            }

            if (!participantRefId.IsNullOrEmpty())
            {
                participant.ParticipantRefId = participantRefId;
            }

            return participant;
        }

        private ITransaction CreateTransaction()
        {
            var transaction = new SilkyTransaction(GuidGenerator.CreateGuidStrWithNoUnderline());
            transaction.Status = ActionStage.PreTry;
            transaction.TransType = TransactionType.Tcc;
            return transaction;
        }

        private ITransaction CreateTransaction(string transId)
        {
            var transaction = new SilkyTransaction(transId);
            transaction.Status = ActionStage.PreTry;
            transaction.TransType = TransactionType.Tcc;
            return transaction;
        }
    }
}