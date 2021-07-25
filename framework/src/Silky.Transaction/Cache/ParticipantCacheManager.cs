using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.Extensions.Caching.Memory;
using Silky.Core;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Cache
{
    public class ParticipantCacheManager
    {
        private static readonly ParticipantCacheManager instance = new();

        //private readonly IEasyCachingProvider _cache;
        private readonly IMemoryCache _cache;
        public const string CacheName = "DefaultInMemory";

        private ParticipantCacheManager()
        {
            //var factory = EngineContext.Current.Resolve<IMemoryCache>();
            _cache = EngineContext.Current.Resolve<IMemoryCache>();
        }

        public static ParticipantCacheManager Instance => instance;

        public async Task CacheParticipant(IParticipant participant)
        {
            await CacheParticipant(participant.ParticipantId, participant);
        }

        public async Task CacheParticipant(string participantId, IParticipant participant)
        {
            var existParticipantList = Get(participantId);
            if (existParticipantList.IsNullOrEmpty())
            {
                var list = new List<IParticipant>() {participant};
                _cache.Set<IList<IParticipant>>(participantId, list, TimeSpan.FromSeconds(60));
            }
            else
            {
                existParticipantList.Add(participant);
                _cache.Set(participantId, existParticipantList,
                    TimeSpan.FromSeconds(60));
            }
        }

        public IList<IParticipant> Get(string participantId)
        {
            var participantsCacheValue = _cache.Get<IList<IParticipant>>(participantId);
            
            return participantsCacheValue;
        }

        public void RemoveByKey(string participantId)
        {
            if (!participantId.IsNullOrEmpty())
            {
                _cache.Remove(participantId);
            }
        }
    }
}