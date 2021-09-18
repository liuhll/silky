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
using Silky.Rpc.Extensions;
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


        public async Task<ITransaction> PreTry(ISilkyMethodInvocation invocation)
        {
            Logger.LogDebug("tcc transaction starter");

            var transaction = CreateTransaction();
            var participant = BuildParticipant(invocation,
                null,
                null,
                TransactionRole.Start,
                transaction.TransId);
            transaction.RegisterParticipant(participant);
            await TransRepositoryStore.CreateTransaction(transaction);
            await TransRepositoryStore.CreateParticipant(participant);

            return transaction;
        }

        public async Task<IParticipant> PreTryParticipant(TransactionContext context,
            ISilkyMethodInvocation invocation)
        {
            Logger.LogDebug($"participant tcc transaction start..：{context}");

            var participant = BuildParticipant(invocation, context.ParticipantId, context.ParticipantRefId,
                TransactionRole.Participant, context.TransId);
            await SilkyTransactionHolder.Instance.CacheParticipant(participant);
            await TransRepositoryStore.CreateParticipant(participant);
            return participant;
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
                    await participant.Executor(ActionStage.Confirming);
                    successList.Add(true);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Participant confirm exception", ex.Message);
                    successList.Add(false);
                }
            }

            if (successList.All(p => true))
            {
                await TransRepositoryStore.RemoveTransaction(currentTransaction);
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
                await participant.Executor(ActionStage.Canceling);
            }
        }

        private IParticipant BuildParticipant(ISilkyMethodInvocation invocation,
            string participantId,
            string participantRefId,
            TransactionRole transactionRole,
            string transId)
        {
            var serviceEntry = invocation.GetServiceEntry();
            var serviceKey = invocation.GetServiceKey();
            var parameters = invocation.GetParameters();

            Debug.Assert(serviceEntry != null);
            if (!serviceEntry.IsTransactionServiceEntry())
            {
                return null;
            }

            if (!serviceEntry.IsLocal)
            {
                return null;
            }

            var isDefinitionConfirmMethod = serviceEntry.IsDefinitionTccMethod(serviceKey, TccMethodType.Confirm);
            if (!isDefinitionConfirmMethod)
            {
                return null;
            }

            var isDefinitionCancelMethod = serviceEntry.IsDefinitionTccMethod(serviceKey, TccMethodType.Cancel);
            if (!isDefinitionCancelMethod)
            {
                return null;
            }

            var participant = new SilkyParticipant()
            {
                Role = transactionRole,
                TransId = transId,
                TransType = TransactionType.Tcc,
                Invocation = invocation,
                ServiceEntryId = serviceEntry.Id,
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
            var transaction = new SilkyTransaction
            {
                TransId = GuidGenerator.CreateGuidStrWithNoUnderline(),
                Status = ActionStage.PreTry,
                TransType = TransactionType.Tcc
            };
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
                await confirmParticipant.Executor(ActionStage.Confirming, invocation);
            }

            ParticipantCacheManager.Instance.RemoveByKey(participantId);
        }

        public async Task ParticipantCancel(ISilkyMethodInvocation invocation,
            IList<IParticipant> cancelingParticipantList, string participantId)
        {
            if (cancelingParticipantList == null) return;

            foreach (var cancelingParticipant in cancelingParticipantList)
            {
                await cancelingParticipant.Executor(ActionStage.Canceling, invocation);
            }

            ParticipantCacheManager.Instance.RemoveByKey(participantId);
        }
    }
}