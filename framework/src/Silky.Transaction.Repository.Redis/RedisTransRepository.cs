using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Silky.Caching;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Repository.Redis
{
    public class RedisTransRepository : ITransRepository
    {
        private const string TransactionCacheKey = "transaction:tcc:{0}";
        private const string ParticipantsCacheKey = "transaction:participant:tcc:{0}:{1}";

        private readonly IDistributedCache<SilkyTransaction> _transactionDistributedCache;
        private readonly IDistributedCache<SilkyParticipant> _participantDistributedCache;

        public RedisTransRepository(IDistributedCache<SilkyTransaction> transactionDistributedCache,
            IDistributedCache<SilkyParticipant> participantDistributedCache)
        {
            _transactionDistributedCache = transactionDistributedCache;
            _participantDistributedCache = participantDistributedCache;
        }


        public async Task SaveTransaction(ITransaction transaction)
        {
            await _transactionDistributedCache.SetAsync(string.Format(TransactionCacheKey, transaction.TransId),
                (SilkyTransaction) transaction);
            var participants = transaction.Participants
                .ToDictionary(k => string.Format(ParticipantsCacheKey, transaction.TransId, k.ParticipantId),
                    v => (SilkyParticipant) v);
            await _participantDistributedCache.SetManyAsync(participants);
        }


        public async Task<ITransaction> FindByTransId(string transId)
        {
            var transaction = await _transactionDistributedCache.GetAsync(string.Format(TransactionCacheKey, transId));
            var participantKeys = await GetParticipantKeys(transId);
            var participants = await _participantDistributedCache.GetManyAsync(participantKeys);
            transaction.RegisterParticipantList(participants.Select(p => p.Value));
            return transaction;
        }

        public async Task UpdateTransactionStatus(string transId, ActionStage status)
        {
            var transaction = await _transactionDistributedCache.GetAsync(string.Format(TransactionCacheKey, transId));
            if (transaction == null)
            {
                throw new TransactionException($"There is no transaction information with id {transId}");
            }

            transaction.Status = status;
            transaction.UpdateTime = DateTime.Now;
            await _transactionDistributedCache.SetAsync(string.Format(TransactionCacheKey, transaction.TransId),
                transaction);
        }

        public async Task RemoveTransaction(string transId)
        {
            await _transactionDistributedCache.RemoveAsync(string.Format(TransactionCacheKey, transId));
            var participantKeys = await GetParticipantKeys(transId);
            foreach (var participantKey in participantKeys)
            {
                await _participantDistributedCache.RemoveAsync(participantKey);
            }
        }

        public Task<int> RemoveTransactionByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public async Task SaveParticipant(IParticipant participant)
        {
            var participantKey = string.Format(ParticipantsCacheKey, participant.TransId, participant.ParticipantId);
            await _participantDistributedCache.SetAsync(participantKey, (SilkyParticipant) participant);
        }

        public async Task<IParticipant> FindParticipant(string transId, string participantId)
        {
            var participantKey = await GetParticipantKey(transId, participantId);
            var participant = await _participantDistributedCache.GetAsync(participantKey);
            return participant;
        }


        public async Task<IEnumerable<IParticipant>> ListParticipantByTransId(string transId)
        {
            var participantKeys = await GetParticipantKeys(transId);
            return (await _participantDistributedCache.GetManyAsync(participantKeys)).Select(p => p.Value);
        }

        public async Task<bool> ExistParticipantByTransId(string transId)
        {
            var participantKeys = await GetParticipantKeys(transId);
            return participantKeys.Count > 0;
        }

        public async Task UpdateParticipantStatus(string transId, string participantId, ActionStage status)
        {
            var participant = await FindParticipant(transId, participantId);
            participant.Status = status;
            participant.UpdateTime = DateTime.Now;
            await SaveParticipant(participant);
          
        }

        public async Task RemoveParticipant(string transId, string participantId)
        {
            var participantKey = await GetParticipantKey(transId, participantId);
            await _participantDistributedCache.RemoveAsync(participantKey);
        }

        public Task<int> RemoveParticipantByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LockParticipant(IParticipant participant)
        {
            throw new NotImplementedException();
        }

        private async Task<IReadOnlyCollection<string>> GetParticipantKeys(string transId)
        {
            var participantsCacheKeyPattern = "*" + string.Format(ParticipantsCacheKey, transId, "*");
            var participantKeys =
                await _participantDistributedCache.SearchKeys(participantsCacheKeyPattern);
            return participantKeys;
        }

        private async Task<string> GetParticipantKey(string transId, string participantId)
        {
            // var participantKeyPattern = "*" + string.Format(ParticipantsCacheKey, "*", participantId) + "*";
            // var participantKeys =
            //     await _participantDistributedCache.SearchKeys(participantKeyPattern);
            // if (participantKeys.Count != 1)
            // {
            //     throw new TransactionException("Failed to obtain the key of transaction participant data");
            // }
            //
            // return participantKeys.First();
            return string.Format(ParticipantsCacheKey, transId, participantId);
        }
    }
}