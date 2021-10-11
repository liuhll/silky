using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Logging;
using Silky.Core.Serialization;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;
using Silky.Transaction.Cache;
using Silky.Transaction.Repository;
using Silky.Transaction.Schedule;

namespace Silky.Transaction.Tcc.Schedule
{
    public class TccTransactionRecoveryService : ITransactionRecoveryService
    {
        public ILogger<TccTransactionRecoveryService> Logger { get; set; }

        private readonly ISerializer _serializer;

        public TccTransactionRecoveryService(ISerializer serializer)
        {
            _serializer = serializer;
            Logger = NullLogger<TccTransactionRecoveryService>.Instance;
        }

        public async Task<bool> Cancel(IParticipant participant)
        {
            try
            {
                await participant.Executor(ActionStage.Canceling);
                ParticipantCacheManager.Instance.RemoveByKey(participant.ParticipantId);
                Logger.LogDebug(
                    $"The transaction participant {participant.ParticipantId} executes the Canceling method successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Recovery executor cancel exception param{_serializer.Serialize(participant)}", ex);
                Logger.LogException(ex);
                return false;
            }
        }

        public async Task<bool> Confirm(IParticipant participant)
        {
            try
            {
                await participant.Executor(ActionStage.Confirming);
                await TransRepositoryStore.RemoveParticipant(participant);
                ParticipantCacheManager.Instance.RemoveByKey(participant.ParticipantId);
                Logger.LogDebug(
                    $"The transaction participant {participant.ParticipantId} executes the Confirming method successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Recovery executor confirm exception param{_serializer.Serialize(participant)}", ex);
                Logger.LogException(ex);
                return false;
            }
        }
    }
}