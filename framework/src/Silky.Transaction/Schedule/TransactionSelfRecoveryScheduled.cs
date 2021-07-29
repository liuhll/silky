using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Serialization;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;
using Silky.Transaction.Configuration;
using Silky.Transaction.Repository;

namespace Silky.Transaction.Schedule
{
    public class TransactionSelfRecoveryScheduled : IHostedService, IDisposable
    {
        private readonly ILogger<TransactionSelfRecoveryScheduled> _logger;
        private readonly ISerializer _serializer;

        private DistributedTransactionOptions _transactionConfig;
        private Timer _selfTccRecoveryTimer;
        private Timer _cleanRecoveryTimer;
        private Timer _phyDeletedTimer;

        public TransactionSelfRecoveryScheduled(ILogger<TransactionSelfRecoveryScheduled> logger,
            ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
            _transactionConfig = EngineContext.Current.GetOptions<DistributedTransactionOptions>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _selfTccRecoveryTimer = new Timer(SelfTccRecovery,
                null,
                TimeSpan.FromSeconds(_transactionConfig.ScheduledInitDelay),
                TimeSpan.FromSeconds(_transactionConfig.ScheduledRecoveryDelay));
            _cleanRecoveryTimer = new Timer(CleanRecovery,
                null,
                TimeSpan.FromSeconds(_transactionConfig.ScheduledInitDelay),
                TimeSpan.FromSeconds(_transactionConfig.ScheduledCleanDelay)
            );
            if (!_transactionConfig.PhyDeleted)
            {
                int seconds = _transactionConfig.StoreDays * 24 * 60 * 60;
                _phyDeletedTimer = new Timer(PhyDeleted,
                    null,
                    TimeSpan.FromSeconds(_transactionConfig.ScheduledInitDelay),
                    TimeSpan.FromSeconds(seconds)
                );
            }

            return Task.CompletedTask;
        }

        private void PhyDeleted([CanBeNull] object state)
        {
        }

        private async void CleanRecovery([CanBeNull] object state)
        {
            try
            {
                var transactionList = await TransRepositoryStore.ListLimitByDelay(AcquireDelayData(_transactionConfig.CleanDelayTime),
                    _transactionConfig.Limit);
                if (transactionList.IsNullOrEmpty())
                {
                    return;
                }

                foreach (var transaction in transactionList)
                {
                    var exist = await TransRepositoryStore.ExistParticipantByTransId(transaction.TransId);
                    if (!exist)
                    {
                        await TransRepositoryStore.RemoveTransaction(transaction);
                    }
                }
                   
            }
            catch (Exception e)
            {
                _logger.LogError($"silky scheduled cleanRecovery log is error{e.Message}", e);
            }
        }

        private async void SelfTccRecovery([CanBeNull] object state)
        {
            try
            {
                var participantList = await TransRepositoryStore.ListParticipant(
                    AcquireDelayData(_transactionConfig.RecoverDelayTime), TransactionType.Tcc,
                    _transactionConfig.Limit);
                if (participantList.IsNullOrEmpty())
                {
                    return;
                }

                foreach (var participant in participantList)
                {
                    if (participant.ReTry > _transactionConfig.RetryMax)
                    {
                        _logger.LogError(
                            $"This tcc transaction exceeds the maximum number of retries and no retries will occur：{_serializer.Serialize(participant)}");
                        participant.Status = ActionStage.Death;
                        await TransRepositoryStore.UpdateParticipantStatus(participant);
                    }

                    if (participant.Status == ActionStage.PreTry)
                    {
                        continue;
                    }

                    var successful = await TransRepositoryStore.LockParticipant(participant);
                    if (successful)
                    {
                        var globalTransaction = await TransRepositoryStore.LoadTransaction(participant.TransId);
                        if (globalTransaction != null)
                        {
                            await TccRecovery(globalTransaction.Status, participant);
                        }
                        else
                        {
                            await TccRecovery(participant.Status, participant);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"silky scheduled transaction log is error{e.Message}", e);
            }
        }

        private async Task TccRecovery(ActionStage stage, IParticipant participant)
        {
            var transactionRecoveryService =
                EngineContext.Current.ResolveNamed<ITransactionRecoveryService>(_transactionConfig.TransactionType
                    .ToString());
            if (stage == ActionStage.Trying || stage == ActionStage.Canceling)
            {
                await transactionRecoveryService.Cancel(participant);
            }
            else if (stage == ActionStage.Confirming)
            {
                await transactionRecoveryService.Confirm(participant);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _selfTccRecoveryTimer?.Change(Timeout.Infinite, 0);
            _cleanRecoveryTimer?.Change(Timeout.Infinite, 0);
            _phyDeletedTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _selfTccRecoveryTimer?.Dispose();
            _cleanRecoveryTimer?.Dispose();
            _phyDeletedTimer?.Dispose();
        }

        private DateTime AcquireDelayData(int delayTime)
        {
            return DateTime.Now.AddSeconds(-delayTime);
        }
    }
}