using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Caching;
using Silky.Core;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Repository.Redis
{
    public class RedisTransRepository : ITransRepository
    {
        private const string TransactionCacheKey = "transaction:{0}";
        private const string ParticipantsCacheKey = "participant:{0}:{1}:{2}";

        private readonly IDistributedCache<SilkyTransaction> _transactionDistributedCache;
        private readonly IDistributedCache<SilkyParticipant> _participantDistributedCache;

        public ILogger<RedisTransRepository> Logger { get; set; }

        private string _hostName;

        public RedisTransRepository(IDistributedCache<SilkyTransaction> transactionDistributedCache,
            IDistributedCache<SilkyParticipant> participantDistributedCache)
        {
            _transactionDistributedCache = transactionDistributedCache;
            _participantDistributedCache = participantDistributedCache;
            _hostName = EngineContext.Current.HostName;
            Logger = NullLogger<RedisTransRepository>.Instance;
        }


        public async Task CreateTransaction(ITransaction transaction)
        {
            var transactionKey = GetTransactionKey(transaction.TransId);
            if (!await _transactionDistributedCache.ExsitAsync(transactionKey))
            {
                transaction.ReTry = 0;
                transaction.Version = 0;
                transaction.CreateTime = DateTime.Now;
            }
            else
            {
                transaction.Version += 1;
            }

            transaction.UpdateTime = DateTime.Now;

            await _transactionDistributedCache.SetAsync(GetTransactionKey(transaction.TransId),
                (SilkyTransaction)transaction);
        }


        public async Task<ITransaction> FindByTransId(string transId)
        {
            var transaction = await _transactionDistributedCache.GetAsync(GetTransactionKey(transId));
            return transaction;
        }

        public async Task UpdateTransactionStatus(string transId, ActionStage status)
        {
            var transaction = await _transactionDistributedCache.GetAsync(GetTransactionKey(transId));
            if (transaction == null)
            {
                throw new TransactionException($"There is no transaction information with id {transId}");
            }

            transaction.Status = status;
            transaction.Version += 1;
            transaction.UpdateTime = DateTime.Now;
            await _transactionDistributedCache.SetAsync(GetTransactionKey(transId), transaction);
        }

        public async Task RemoveTransaction(string transId)
        {
            await _transactionDistributedCache.RemoveAsync(GetTransactionKey(transId));
        }

        public async Task<int> RemoveTransactionByDate(DateTime date)
        {
            var transactionKeyPattern = "*" + GetTransactionKey("*");
            var transactionKeys = await _participantDistributedCache.SearchKeys(transactionKeyPattern);
            if (transactionKeys.IsNullOrEmpty())
            {
                return 0;
            }

            var transactions = (await _transactionDistributedCache.GetManyAsync(transactionKeys)).Select(p => p.Value);

            var removeTransactions = transactions.Where(p => p.UpdateTime < date && p.Status == ActionStage.Delete)
                .ToArray();

            foreach (var removeTransaction in removeTransactions)
            {
                await RemoveTransaction(removeTransaction.TransId);
            }

            return removeTransactions.Length;
        }

        public async Task CreateParticipant(IParticipant participant)
        {
            var participantKey = GetParticipantKey(participant.TransId, participant.ParticipantId);
            var exsit = await _participantDistributedCache.ExsitAsync(participantKey);
            if (!exsit)
            {
                participant.Version = 0;
                participant.ReTry = 0;
                participant.CreateTime = DateTime.Now;
            }
            else
            {
                participant.Version += 1;
            }

            participant.UpdateTime = DateTime.Now;
            await _participantDistributedCache.SetAsync(participantKey, (SilkyParticipant)participant);
        }

        public async Task<IReadOnlyCollection<IParticipant>> FindParticipant(string transId, string participantId)
        {
            var transParticipant = await ListParticipantByTransId(transId);
            return transParticipant.Where(p => p.ParticipantId == participantId
                                               || (p.ParticipantRefId != null && p.ParticipantRefId == participantId))
                .ToArray();
        }


        public async Task<IReadOnlyCollection<IParticipant>> ListParticipantByTransId(string transId)
        {
            var participantKeys = await GetParticipantKeys(transId);
            var participantCaches = await _participantDistributedCache.GetManyAsync(participantKeys);

            return participantCaches.Select(p => p.Value).ToArray();
        }

        public async Task<bool> ExistParticipantByTransId(string transId)
        {
            var participantKeys = await GetParticipantKeys(transId);
            var participants = (await _participantDistributedCache.GetManyAsync(participantKeys)).Select(p => p.Value);

            return participants.Count(p => p.Status != ActionStage.Delete) > 0;
        }

        public async Task UpdateParticipantStatus(string transId, string participantId, ActionStage status)
        {
            var participantKey = GetParticipantKey(transId, participantId);
            var participant = await _participantDistributedCache.GetAsync(participantKey);
            if (participant == null)
            {
                throw new TransactionException($"There is no participant information with id {participantId}");
            }

            participant.Version += 1;
            participant.Status = status;
            participant.UpdateTime = DateTime.Now;
            await _participantDistributedCache.SetAsync(participantKey, (SilkyParticipant)participant);
        }

        public async Task RemoveParticipant(string transId, string participantId)
        {
            var participantKey = GetParticipantKey(transId, participantId);
            await _participantDistributedCache.RemoveAsync(participantKey);
        }

        public async Task<int> RemoveParticipantByDate(DateTime date)
        {
            var participantKeys = await GetParticipantKeys("*");
            if (participantKeys.IsNullOrEmpty())
            {
                return 0;
            }

            var participants = (await _participantDistributedCache.GetManyAsync(participantKeys)).Select(p => p.Value);
            var removeParticipants = participants.Where(p => p.UpdateTime < date && p.Status == ActionStage.Delete)
                .ToArray();
            foreach (var removeParticipant in removeParticipants)
            {
                await RemoveParticipant(removeParticipant.TransId, removeParticipant.ParticipantId);
            }

            return removeParticipants.Length;
        }

        public async Task<bool> LockParticipant(IParticipant participant)
        {
            var currentVersion = participant.Version;
            var key = GetParticipantKey(participant.TransId, participant.ParticipantId);
            try
            {
                var existParticipant = await _participantDistributedCache.GetAsync(key);
                if (existParticipant == null || existParticipant.Status == ActionStage.Delete ||
                    existParticipant.Status == ActionStage.Death)
                {
                    Logger.LogWarning($"key {key} is not exists.");
                    return false;
                }

                participant.Version = currentVersion + 1;
                participant.ReTry += 1;
                participant.UpdateTime = DateTime.Now;
                await _participantDistributedCache.SetAsync(key, (SilkyParticipant)participant);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Lock participant occur a exception", ex);
                return false;
            }
        }

        public async Task<IReadOnlyCollection<IParticipant>> ListParticipant(DateTime dateTime,
            TransactionType transactionType,
            int limit)
        {
            var participants = await ListParticipantByTransId("*");
            if (participants == null)
            {
                return new List<IParticipant>(0);
            }

            return participants.Where(p => p.TransType == transactionType
                                           && p.UpdateTime >= dateTime
                                           && p.HostName == _hostName
                                           && p.Status != ActionStage.Delete
                                           && p.Status != ActionStage.Death
                ).Take(limit)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<ITransaction>> ListLimitByDelay(DateTime dateTime, int limit)
        {
            var transactionKeyPattern = "*" + GetTransactionKey("*");
            var transactionKeys = await _participantDistributedCache.SearchKeys(transactionKeyPattern);

            var transactions = (await _transactionDistributedCache.GetManyAsync(transactionKeys)).Select(p => p.Value);
            return transactions
                .Where(p => p.UpdateTime > dateTime
                            && p.Status != ActionStage.Delete
                            && p.HostName == _hostName)
                .Take(limit)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<string>> GetParticipantKeys(string transId)
        {
            var participantsCacheKeyPattern = "*" + GetParticipantKey(transId, "*");
            var participantKeys =
                await _participantDistributedCache.SearchKeys(participantsCacheKeyPattern);
            return participantKeys.ToArray();
        }

        private string GetParticipantKey(string transId, string participantId)
        {
            return string.Format(ParticipantsCacheKey, _hostName, transId, participantId);
        }

        private string GetTransactionKey(string transId)
        {
            return string.Format(TransactionCacheKey, transId);
        }
    }
}