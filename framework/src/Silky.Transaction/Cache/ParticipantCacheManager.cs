using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Silky.Caching;
using Silky.Core;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Serialization;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Cache
{
    public class ParticipantCacheManager
    {
        private readonly IDistributedCache<string> _cache;
        private readonly ISerializer _serializer;

        private ParticipantCacheManager()
        {
            _cache = EngineContext.Current.Resolve<IDistributedCache<string>>();
            _serializer = EngineContext.Current.Resolve<ISerializer>();
        }

        public static ParticipantCacheManager Instance =>
            Singleton<ParticipantCacheManager>.Instance ??
            (Singleton<ParticipantCacheManager>.Instance = new ParticipantCacheManager());

        public async Task CacheParticipant(IParticipant participant)
        {
            await CacheParticipant(participant.ParticipantId, participant);
        }

        public async Task CacheParticipant(string participantId, IParticipant participant)
        {
            var existParticipantList = await Get(participantId);
            string cacheValue = string.Empty;
            if (existParticipantList.IsNullOrEmpty())
            {
                var list = new List<IParticipant>() { participant };
                cacheValue = _serializer.Serialize(list);
            }
            else
            {
                existParticipantList.Add(participant);
                cacheValue = _serializer.Serialize(existParticipantList);
            }

            _cache.Set(participantId, cacheValue);
        }

        public async Task<IList<IParticipant>> Get(string participantId)
        {
            var participantsCacheValue = _cache.Get(participantId);
            if (participantsCacheValue != null)
            {
                var participants = _serializer.Deserialize<IList<SilkyParticipant>>(participantsCacheValue);
                return participants.Select(p => (IParticipant)p).ToList();
            }

            return null;
        }

        public async Task RemoveByKey(string participantId)
        {
            if (!participantId.IsNullOrEmpty())
            {
                await _cache.RemoveAsync(participantId);
            }
        }
    }
}