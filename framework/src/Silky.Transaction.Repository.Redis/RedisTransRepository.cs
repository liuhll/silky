using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silky.Caching;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Repository.Redis
{
    public class RedisTransRepository : ITransRepository
    {
        private const string TransactionCacheKey = "transaction:tcc:{0}";
        private const string ParticipantsCacheKey = "transaction:participant:tcc:{0}:{1}";

        private readonly IDistributedCache<ITransaction> _transactionDistributedCache;
        private readonly IDistributedCache<IParticipant> _participantDistributedCache;

        public RedisTransRepository(IDistributedCache<ITransaction> transactionDistributedCache,
            IDistributedCache<IParticipant> participantDistributedCache)
        {
            _transactionDistributedCache = transactionDistributedCache;
            _participantDistributedCache = participantDistributedCache;
        }


        public async Task SaveTransaction(ITransaction transaction)
        {
            await _transactionDistributedCache.SetAsync(string.Format(TransactionCacheKey, transaction.TransId),
                transaction);
            await _participantDistributedCache.SetManyAsync(transaction.Participants.Select(
                p => new KeyValuePair<string, IParticipant>(
                    string.Format(ParticipantsCacheKey, transaction.TransId, p.ParticipantId), p)));
        }

        
        public async Task<ITransaction> FindByTransId(string transId)
        {
            var transaction = await _transactionDistributedCache.GetAsync(string.Format(TransactionCacheKey, transId));
            var participantKeys = await _participantDistributedCache.SearchKeys(string.Format(ParticipantsCacheKey,transId,"*"));
            var participants = await _participantDistributedCache.GetManyAsync(participantKeys);
            transaction.RegisterParticipantList(participants.Select(p=> p.Value));
            return transaction;
        }

        public Task UpdateTransactionStatus(string transId, ActionStage status)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTransaction(string transId)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveTransactionByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateParticipant(IParticipant participant)
        {
            throw new NotImplementedException();
        }

        public Task<IParticipant> FindParticipant(string participantId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IParticipant>> ListParticipant(DateTime date, TransactionType transType, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IParticipant>> ListParticipantByTransId(string transId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistParticipantByTransId(string transId)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateParticipantStatus(string participantId, ActionStage status)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveParticipant(string participantId)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveParticipantByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LockParticipant(IParticipant participant)
        {
            throw new NotImplementedException();
        }
    }
}