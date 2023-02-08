using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using JetBrains.Annotations;
using Medallion.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Logging;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.DistributedLock.Redis;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;
using Silky.Transaction.Configuration;
using Silky.Transaction.Repository;
using StackExchange.Redis;

namespace Silky.Transaction.Schedule
{
    public class TransactionSelfRecoveryScheduled : IHostedService, IAsyncDisposable
    {
        private readonly ILogger<TransactionSelfRecoveryScheduled> _logger;
        private readonly ISerializer _serializer;
        private IRedisDistributedLockProvider _distributedLockProvider;
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
            _distributedLockProvider = EngineContext.Current.Resolve<IRedisDistributedLockProvider>();
            if (_distributedLockProvider == null)
            {
                throw new SilkyException("Failed to create distributed lock provider", StatusCode.TransactionError);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
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
        }

        private async void PhyDeleted([CanBeNull] object state)
        {
            if (!_transactionConfig.PhyDeleted)
            {
                try
                {
                    var redisOptions = EngineContext.Current.Configuration.GetRedisCacheOptions();
                    using var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.Configuration);
                    var @lock = _distributedLockProvider.Create(connection.GetDatabase(), LockName.PhyDeleted);
                    await using var handle = await @lock.TryAcquireAsync();
                    if (handle == null)
                    {
                        _logger.LogDebug($"Silky scheduled phyDeleted failed to acquire distributed lock");
                    }
                    else
                    {
                        var seconds = _transactionConfig.StoreDays * 24 * 60 * 60;
                        var removeTransCount =
                            await TransRepositoryStore.RemoveTransactionByDate(AcquireDelayData(seconds));
                        var removeParticipantCount =
                            await TransRepositoryStore.RemoveParticipantByDate(AcquireDelayData(seconds));
                        _logger.LogDebug(
                            "silky scheduled phyDeleted => transaction:{RemoveTransCount},participant:{RemoveParticipantCount}",
                            removeTransCount, removeParticipantCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("silky scheduled phyDeleted log is error{ExMessage}", ex, ex.Message);
                    _logger.LogException(ex);
                }
            }
        }

        private async void CleanRecovery([CanBeNull] object state)
        {
            try
            {
                var redisOptions = EngineContext.Current.Configuration.GetRedisCacheOptions();
                using var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.Configuration);
                var @lock = _distributedLockProvider.Create(connection.GetDatabase(), LockName.CleanRecovery);
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
                        var @transactionLock = _distributedLockProvider.Create(connection.GetDatabase(),
                            LockName.CleanRecoveryTransaction +
                            transaction.TransId);
                        await using var transactionHandle = await @transactionLock.AcquireAsync();
                        var exist = await TransRepositoryStore.ExistParticipantByTransId(transaction.TransId);
                        if (!exist)
                        {
                            await TransRepositoryStore.RemoveTransaction(transaction);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("silky scheduled cleanRecovery log is error{ExMessage}", ex, ex.Message);
                _logger.LogException(ex);
            }
        }

        private async void SelfTccRecovery([CanBeNull] object state)
        {
            try
            {
                var redisOptions = EngineContext.Current.Configuration.GetRedisCacheOptions();
                using var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.Configuration);
                var @lock = _distributedLockProvider.Create(connection.GetDatabase(), LockName.SelfTccRecovery);

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
                        var participantLock = _distributedLockProvider.Create(connection.GetDatabase(),
                            LockName.ParticipantTccRecovery +
                            participant.ParticipantId);
                        await using var participantHandle =
                            await participantLock.TryAcquireAsync();

                        if (participantHandle == null) continue;
                        if (participant.ReTry > _transactionConfig.RetryMax)
                        {
                            _logger.LogError(
                                "This tcc transaction exceeds the maximum number of retries and no retries will occur：{Serialize}",
                                _serializer.Serialize(participant));
                            participant.Status = ActionStage.Death;
                            await TransRepositoryStore.UpdateParticipantStatus(participant);
                        }

                        if (participant.Status == ActionStage.PreTry)
                        {
                            continue;
                        }

                        var successful = await TransRepositoryStore.LockParticipant(participant);
                        if (!successful) continue;
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
            catch (Exception ex)
            {
                _logger.LogError("silky scheduled SelfTccRecovery log is error{ExMessage}", ex.Message);
                _logger.LogException(ex);
            }
        }

        private async Task TccRecovery(ActionStage stage, IParticipant participant)
        {
            var transactionRecoveryService =
                EngineContext.Current.ResolveNamed<ITransactionRecoveryService>(_transactionConfig
                    .TransactionType
                    .ToString());
            RpcContext.Context.SetInvokeAttachments(participant.InvokeAttachments);
            switch (stage)
            {
                case ActionStage.Trying:
                case ActionStage.Canceling:
                    await transactionRecoveryService.Cancel(participant);
                    break;
                case ActionStage.Confirming:
                    await transactionRecoveryService.Confirm(participant);
                    break;
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
        }

        private DateTime AcquireDelayData(int delayTime)
        {
            return DateTime.Now.AddSeconds(-delayTime);
        }

        public async ValueTask DisposeAsync()
        {
            if (_selfTccRecoveryTimer != null)
            {
                await _selfTccRecoveryTimer.DisposeAsync();
            }

            if (_cleanRecoveryTimer != null)
            {
                await _cleanRecoveryTimer.DisposeAsync();
            }

            if (_phyDeletedTimer != null)
            {
                await _phyDeletedTimer.DisposeAsync();
            }
        }
    }
}