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
        private static TccTransactionExecutor executor = new ();
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
            var context = new TransactionContext();
            //set action is try
            context.Action = ActionStage.Trying;
            context.TransId = transaction.TransId;
            context.TransactionRole = TransactionRole.Start;
            context.TransType = TransactionType.Tcc;
            RpcContext.GetContext().SetAttachment("transactionContext", context);

            return transaction;
        }

        private IParticipant BuildParticipant(ILmsMethodInvocation invocation, string participantId,
            string participantRefId, TransactionRole transactionRole, string transId)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            Debug.Assert(serviceEntry != null);
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var tccTransactionProvider = serviceEntry.GetTccTransactionProvider(serviceKey);
            if (tccTransactionProvider == null)
            {
                return null;
            }

            var participant = new LmsParticipant()
            {
                Role = transactionRole,
                TransId = transId,
                TransType = TransactionType.Tcc,
                ConfirmMethod = tccTransactionProvider.ConfirmMethod,
                CancelMethod = tccTransactionProvider.CancelMethod
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