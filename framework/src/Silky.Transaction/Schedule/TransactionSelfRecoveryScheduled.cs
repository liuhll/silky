using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using JetBrains.Annotations;
using Medallion.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
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
        private IDistributedLockProvider _distributedLockProvider;
        private DistributedTransactionOptions _transactionConfig;
        private Timer _selfTccRecoveryTimer;
        private Timer _cleanRecoveryTimer;
        private Timer _phyDeletedTimer;

        public TransactionSelfRecoveryScheduled(ILogger<TransactionSelfRecoveryScheduled> logger,
            ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
            _transactionConfig =
                EngineContext.Current.GetOptionsMonitor<DistributedTransactionOptions>((options, s) =>
                    _transactionConfig = options);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _distributedLockProvider = EngineContext.Current.Resolve<IDistributedLockProvider>();
            if (_distributedLockProvider == null)
            {
                throw new SilkyException("Failed to create distributed lock provider", StatusCode.TransactionError);
            }

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
        }

        private async void PhyDeleted([CanBeNull] object state)
        {
            if (!_transactionConfig.PhyDeleted)
            {
                try
                {
                    var @lock = _distributedLockProvider.CreateLock(LockName.PhyDeleted);
                    await using var handle = await @lock.TryAcquireAsync();
                    if (handle != null)
                    {
                        var seconds = _transactionConfig.StoreDays * 24 * 60 * 60;
                        var removeTransCount =
                            await TransRepositoryStore.RemoveTransactionByDate(AcquireDelayData(seconds));
                        var removeParticipantCount =
                            await TransRepositoryStore.RemoveParticipantByDate(AcquireDelayData(seconds));
                        _logger.LogDebug(
                            $"silky scheduled phyDeleted => transaction:{removeTransCount},participant:{removeParticipantCount}");
                    }
                    else
                    {
                        _logger.LogWarning($"Silky scheduled phyDeleted failed to acquire distributed lock");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"silky scheduled phyDeleted log is error{ex.Message}", ex);
                    _logger.LogException(ex);
                }
            }
        }

        private async void CleanRecovery([CanBeNull] object state)
        {
            try
            {
                var @lock = _distributedLockProvider.CreateLock(LockName.CleanRecovery);
                await using var handle = await @lock.TryAcquireAsync();
                if (handle != null)
                {
                    var transactionList = await TransRepositoryStore.ListLimitByDelay(
                        AcquireDelayData(_transactionConfig.CleanDelayTime),
                        _transactionConfig.Limit);
                    if (transactionList.IsNullOrEmpty())
                    {
                        return;
                    }

                    foreach (var transaction in transactionList)
                    {
                        var transactionLock =
                            _distributedLockProvider.CreateLock(string.Format(LockName.CleanRecoveryTransaction));
                        await using var transactionLockHandle =
                            await transactionLock.TryAcquireAsync();
                        if (transactionLockHandle != null)
                        {
                            var exist = await TransRepositoryStore.ExistParticipantByTransId(transaction.TransId);
                            if (!exist)
                            {
                                await TransRepositoryStore.RemoveTransaction(transaction);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Silky scheduled cleanRecovery failed to acquire distributed lock");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"silky scheduled cleanRecovery log is error{ex.Message}", ex);
                _logger.LogException(ex);
            }
        }

        private async void SelfTccRecovery([CanBeNull] object state)
        {
            try
            {
                var @lock = _distributedLockProvider.CreateLock(LockName.SelfTccRecovery);
                await using var handle = await @lock.TryAcquireAsync();
                if (handle != null)
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
                        var @participantLock = _distributedLockProvider.CreateLock(LockName.ParticipantTccRecovery);
                        await using var participantHandle =
                            await @participantLock.TryAcquireAsync();
                        if (participantHandle != null)
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
                        else
                        {
                            _logger.LogWarning($"Silky scheduled TccRecovery failed to acquire distributed lock");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Silky scheduled SelfTccRecovery failed to acquire distributed lock");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"silky scheduled SelfTccRecovery log is error{ex.Message}", ex);
                _logger.LogException(ex);
            }
        }

        private async Task TccRecovery(ActionStage stage, IParticipant participant)
        {
            var transactionRecoveryService =
                EngineContext.Current.ResolveNamed<ITransactionRecoveryService>(_transactionConfig
                    .TransactionType
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