using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Silky.Core.DynamicProxy;
using Silky.Core.Extensions;
using Silky.Core.Utils;
using Silky.Rpc.Runtime.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Transaction.Cache;
using Silky.Transaction.Repository;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;

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


        public async Task<(ITransaction, TransactionContext)> PreTry(ISilkyMethodInvocation invocation)
        {
            Logger.LogDebug("tcc transaction starter");

            var transaction = CreateTransaction();
            var participant = BuildParticipant(invocation,
                null,
                null,
                TransactionRole.Start,
                transaction.TransId);
            transaction.RegisterParticipant(participant);
            await TransRepositoryStore.SaveTransaction(transaction);
            var context = new TransactionContext
            {
                Action = ActionStage.Trying,
                TransId = transaction.TransId,
                TransactionRole = TransactionRole.Start,
                TransType = TransactionType.Tcc
            };


            return (transaction, context);
        }

        public async Task<(IParticipant, TransactionContext)> PreTryParticipant(TransactionContext context,
            ISilkyMethodInvocation invocation)
        {
            Logger.LogDebug($"participant tcc transaction start..：{context}");

            var participant = BuildParticipant(invocation, context.ParticipantId, context.ParticipantRefId,
                TransactionRole.Participant, context.TransId);
            await SilkyTransactionHolder.Instance.CacheParticipant(participant);
            await TransRepositoryStore.SaveParticipant(participant);
            context.TransactionRole = TransactionRole.Participant;
            //  SilkyTransactionContextHolder.Instance.Set(context);
            return (participant, context);
        }

        public async Task GlobalConfirm(ITransaction currentTransaction)
        {
            Logger.LogDebug("tcc transaction confirm .......！start");
            if (currentTransaction == null || currentTransaction.Participants.IsNullOrEmpty())
            {
                return;
            }

            currentTransaction.Status = ActionStage.Confirming;
            await TransRepositoryStore.UpdateTransactionStatus(currentTransaction);
            var successList = new List<bool>();

            foreach (var participant in currentTransaction.Participants)
            {
                try
                {
                    await participant.ParticipantConfirm();
                    successList.Add(true);
                    participant.Status = ActionStage.Confirmed;
                    await TransRepositoryStore.UpdateParticipantStatus(participant);
                }
                catch (Exception e)
                {
                    Logger.LogError("Participant confirm exception", e.Message);
                    successList.Add(false);
                }
            }

            if (successList.All(p => true))
            {
                currentTransaction.Status = ActionStage.Confirmed;
                await TransRepositoryStore.UpdateTransactionStatus(currentTransaction);
            }
        }

        public async Task GlobalCancel(ITransaction currentTransaction)
        {
            Logger.LogDebug("tcc transaction cancel .......！start");
            if (currentTransaction == null || currentTransaction.Participants.IsNullOrEmpty())
            {
                return;
            }

            currentTransaction.Status = ActionStage.Canceling;
            await TransRepositoryStore.UpdateTransactionStatus(currentTransaction);
            foreach (var participant in currentTransaction.Participants)
            {
                await participant.ParticipantCancel();
            }
        }

        private IParticipant BuildParticipant(ISilkyMethodInvocation invocation,
            string participantId,
            string participantRefId,
            TransactionRole transactionRole,
            string transId)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];

            Debug.Assert(serviceEntry != null);
            if (!serviceEntry.IsTransactionServiceEntry())
            {
                return null;
            }

            if (!serviceEntry.IsLocal)
            {
                return null;
            }

            var confirmMethodInfo = serviceEntry.GetTccExcutorInfo(serviceKey, TccMethodType.Confirm);
            if (confirmMethodInfo.Item1 == null)
            {
                return null;
            }

            var cancelMethodInfo = serviceEntry.GetTccExcutorInfo(serviceKey, TccMethodType.Cancel);
            if (cancelMethodInfo.Item1 == null)
            {
                return null;
            }

            var participant = new SilkyParticipant()
            {
                Role = transactionRole,
                TransId = transId,
                TransType = TransactionType.Tcc,
                Invocation = invocation,
                ServiceId = serviceEntry.Id,
                Status = ActionStage.PreTry,
                ServiceKey = serviceKey,
                Parameters = parameters
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
            var transaction = new SilkyTransaction();
            transaction.TransId = GuidGenerator.CreateGuidStrWithNoUnderline();
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

        public async Task UpdateStartStatus(ITransaction transaction)
        {
            await TransRepositoryStore.UpdateTransactionStatus(transaction);
            var participant = FilterStartParticipant(transaction);
            if (participant != null)
            {
                participant.Status = transaction.Status;
                await TransRepositoryStore.UpdateParticipantStatus(participant);
            }
        }

        public void Remove()
        {
            SilkyTransactionHolder.Instance.Remove();
        }

        private IParticipant FilterStartParticipant(ITransaction transaction)
        {
            if (transaction.Participants.IsNullOrEmpty())
            {
                return null;
            }

            return transaction.Participants.FirstOrDefault(p => p.Role == TransactionRole.Start);
        }

        public async Task ParticipantConfirm(ISilkyMethodInvocation invocation,
            IList<IParticipant> confirmingParticipantList, string participantId)

        {
            if (confirmingParticipantList == null) return;

            foreach (var confirmParticipant in confirmingParticipantList)
            {
                await confirmParticipant.ParticipantConfirm(invocation);
            }

            ParticipantCacheManager.Instance.RemoveByKey(participantId);
        }

        public async Task ParticipantCancel(ISilkyMethodInvocation invocation,
            IList<IParticipant> cancelingParticipantList, string participantId)
        {
            if (cancelingParticipantList == null) return;

            var selfParticipant = cancelingParticipantList.FirstOrDefault(p => p.ParticipantRefId != null);
            if (selfParticipant != null)
            {
                selfParticipant.Status = ActionStage.Canceling;
                await TransRepositoryStore.UpdateParticipantStatus(selfParticipant);
            }

            foreach (var cancelingParticipant in cancelingParticipantList)
            {
                await cancelingParticipant.ParticipantCancel(invocation);
            }

            ParticipantCacheManager.Instance.RemoveByKey(participantId);
        }
    }
}