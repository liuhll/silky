using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Silky.Core;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Serialization;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Cache
{
    public class ParticipantCacheManager
    {
        private readonly IDistributedCache _cache;
        private readonly ISerializer _serializer;
        private const string ParticipantKey = "Participant:{0}";

        private ParticipantCacheManager()
        {
            _cache = EngineContext.Current.Resolve<IDistributedCache>();
            _serializer = EngineContext.Current.Resolve<ISerializer>();
        }

        public static ParticipantCacheManager Instance =>
            Singleton<ParticipantCacheManager>.Instance ??
            (Singleton<ParticipantCacheManager>.Instance = new ParticipantCacheManager());

        public void CacheParticipant(IParticipant participant)
        {
            CacheParticipant(participant.ParticipantId, participant);
        }

        public void CacheParticipant(string participantId, IParticipant participant)
        {
            var existParticipantList = Get(participantId);
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

            _cache.SetString(CacheKey(participantId), cacheValue);
        }

        public IList<IParticipant> Get(string participantId)
        {
            var participantsCacheValue = _cache.GetString(CacheKey(participantId));
            if (participantsCacheValue != null)
            {
                var participants = _serializer.Deserialize<IList<SilkyParticipant>>(participantsCacheValue);
                return participants.Select(p => (IParticipant)p).ToList();
            }

            return null;
        }

        public void RemoveByKey(string participantId)
        {
            if (!participantId.IsNullOrEmpty())
            {
                _cache.Remove(CacheKey(participantId));
            }
        }

        private string CacheKey(string participantId)
        {
            return string.Format(ParticipantKey, participantId);
        }
    }
}