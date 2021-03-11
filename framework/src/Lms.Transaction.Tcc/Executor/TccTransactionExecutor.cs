using System.Diagnostics;
using Lms.Core.DynamicProxy;
using Lms.Core.Extensions;
using Lms.Core.Utils;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transaction;
using Lms.Rpc.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Transaction.Tcc.Executor
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


        public ITransaction PreTry(ILmsMethodInvocation invocation)
        {
            Logger.LogDebug("tcc transaction starter");
            var transaction = CreateTransaction();
            var participant = BuildParticipant(invocation, null, null, TransactionRole.Start, transaction.TransId);
            transaction.RegisterParticipant(participant);
            LmsTransactionHolder.Instance.Set(transaction);
            var context = new TransactionContext
            {
                Action = ActionStage.Trying,
                TransId = transaction.TransId,
                TransactionRole = TransactionRole.Start,
                TransType = TransactionType.Tcc
            };
            //set action is try
            RpcContext.GetContext().SetAttachment("transactionContext", context);

            return transaction;
        }

        public IParticipant PreTryParticipant(TransactionContext context, ILmsMethodInvocation invocation)
        {
            Logger.LogDebug($"participant tcc transaction start..：{context}");
            IParticipant participant = BuildParticipant(invocation, context.ParticipantId,
                context.ParticipantId, TransactionRole.Participant, context.TransId);
            LmsTransactionHolder.Instance.GetCurrentTransaction().RegisterParticipant(participant);
            context.TransactionRole = TransactionRole.Participant;
            //ContextHolder.set(context);
            return participant;
        }

        private IParticipant BuildParticipant(ILmsMethodInvocation invocation, string participantId,
            string participantRefId, TransactionRole transactionRole, string transId)
        {
            // var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            // Debug.Assert(serviceEntry != null);
            // var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            // var tccTransactionProvider = serviceEntry.GetTccTransactionProvider(serviceKey);
            // if (tccTransactionProvider == null)
            // {
            //     return null;
            // }

            var participant = new LmsParticipant()
            {
                Role = transactionRole,
                TransId = transId,
                TransType = TransactionType.Tcc,
                // ConfirmMethod = tccTransactionProvider.ConfirmMethod,
                // CancelMethod = tccTransactionProvider.CancelMethod
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
            var transaction = new LmsTransaction(GuidGenerator.CreateGuidStrWithNoUnderline());
            transaction.Status = ActionStage.PreTry;
            transaction.TransType = TransactionType.Tcc;
            return transaction;
        }
    }
}