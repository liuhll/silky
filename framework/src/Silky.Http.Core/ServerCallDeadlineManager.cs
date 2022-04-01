using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Logging;

namespace Silky.Http.Core;

internal sealed class ServerCallDeadlineManager : IAsyncDisposable
{
    private const long DefaultMaxTimerDueTime = uint.MaxValue - 1;

    private static readonly TimerCallback DeadlineExceededDelegate = DeadlineExceededCallback;
    private static readonly TimerCallback DeadlineExceededLongDelegate = DeadlineExceededLongCallback;

    private readonly Timer _longDeadlineTimer;
    private readonly ISystemClock _systemClock;
    private readonly HttpContextServerCallContext _serverCallContext;

    private CancellationTokenSource? _deadlineCts;
    private CancellationTokenRegistration _requestAbortedRegistration;
    private TaskCompletionSource<object?>? _deadlineExceededCompleteTcs;

    public DateTime Deadline { get; }
    public bool IsCallComplete { get; private set; }
    public bool IsDeadlineExceededStarted => _deadlineExceededCompleteTcs != null;

    public CancellationToken CancellationToken
    {
        get
        {
            // Lazy create a CT only when requested for performance
            if (_deadlineCts == null)
            {
                lock (this)
                {
                    // Double check locking
                    if (_deadlineCts == null)
                    {
                        _deadlineCts = new CancellationTokenSource();
                        if (IsDeadlineExceededStarted && IsCallComplete)
                        {
                            // If deadline has started exceeding and it has finished then the token can be immediately cancelled
                            _deadlineCts.Cancel();
                        }
                        else
                        {
                            // Deadline CT should be cancelled if the request is aborted
                            _requestAbortedRegistration =
                                _serverCallContext.HttpContext.RequestAborted.Register(RequestAborted);
                        }
                    }
                }
            }

            return _deadlineCts.Token;
        }
    }

    public ServerCallDeadlineManager(HttpContextServerCallContext serverCallContext, ISystemClock clock,
        TimeSpan timeout, long maxTimerDueTime = DefaultMaxTimerDueTime)
    {
        // Set fields that need to exist before setting up deadline CTS
        // Ensures callback can run successfully before CTS timer starts
        _serverCallContext = serverCallContext;

        Deadline = clock.UtcNow.Add(timeout);

        _systemClock = clock;

        var timerMilliseconds = CommonSilkyHelpers.GetTimerDueTime(timeout, maxTimerDueTime);
        if (timerMilliseconds == maxTimerDueTime)
        {
            // Create timer and set to field before setting time.
            // Ensures there is no weird situation where the timer triggers
            // before the field is set. Shouldn't happen because only long deadlines
            // will take this path but better to be safe than sorry.
            _longDeadlineTimer = new Timer(DeadlineExceededLongDelegate, (this, maxTimerDueTime), Timeout.Infinite,
                Timeout.Infinite);
            _longDeadlineTimer.Change(timerMilliseconds, Timeout.Infinite);
        }
        else
        {
            _longDeadlineTimer = new Timer(DeadlineExceededDelegate, this, timerMilliseconds, Timeout.Infinite);
        }
    }

    public Task WaitDeadlineCompleteAsync()
    {
        Debug.Assert(_deadlineExceededCompleteTcs != null, "Can only be called if deadline is started.");

        return _deadlineExceededCompleteTcs.Task;
    }

    private static void DeadlineExceededCallback(object? state) =>
        _ = ((ServerCallDeadlineManager)state!).DeadlineExceededAsync();

    private static void DeadlineExceededLongCallback(object? state)
    {
        var (manager, maxTimerDueTime) = (ValueTuple<ServerCallDeadlineManager, long>)state!;
        var remaining = manager.Deadline - manager._systemClock.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            _ = manager.DeadlineExceededAsync();
        }
        else
        {
            // Deadline has not been reached because timer maximum due time was smaller than deadline.
            // Reschedule DeadlineExceeded again until deadline has been exceeded.
            manager._serverCallContext.Logger.LogDebug(
                $"Deadline timer triggered but {0} remaining before deadline exceeded. Deadline timer rescheduled.",
                remaining);

            manager._longDeadlineTimer.Change(CommonSilkyHelpers.GetTimerDueTime(remaining, maxTimerDueTime),
                Timeout.Infinite);
        }
    }

    private void RequestAborted()
    {
        // Call is complete if the request has aborted
        lock (this)
        {
            IsCallComplete = true;
        }

        // Doesn't matter if error from Cancel throws. Canceller of request aborted will handle exception.
        Debug.Assert(_deadlineCts != null, "Deadline CTS is created when request aborted method is registered.");
        _deadlineCts.Cancel();
    }

    public void SetCallEnded()
    {
        lock (this)
        {
            IsCallComplete = true;
        }
    }

    public bool TrySetCallComplete()
    {
        lock (this)
        {
            if (!IsDeadlineExceededStarted)
            {
                IsCallComplete = true;
                return true;
            }

            return false;
        }
    }

    private async Task DeadlineExceededAsync()
    {
        if (!TryStartExceededDeadline())
        {
            return;
        }

        try
        {
            await _serverCallContext.DeadlineExceededAsync();
            lock (this)
            {
                IsCallComplete = true;
            }

            // Canceling CTS will trigger registered callbacks.
            // Exception could be thrown from them.
            _deadlineCts?.Cancel();
        }
        catch (Exception ex)
        {
            _serverCallContext.Logger.LogException(ex);
        }
        finally
        {
            _deadlineExceededCompleteTcs!.TrySetResult(null);
        }
    }

    private bool TryStartExceededDeadline()
    {
        lock (this)
        {
            // Deadline callback could be raised by the CTS after call has been completed (either successfully, with error, or aborted)
            // but before deadline exceeded registration has been disposed
            if (!IsCallComplete)
            {
                _deadlineExceededCompleteTcs =
                    new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                return true;
            }

            return false;
        }
    }


    public ValueTask DisposeAsync()
    {
        var disposeTask = _longDeadlineTimer.DisposeAsync();

        if (disposeTask.IsCompletedSuccessfully &&
            (_deadlineExceededCompleteTcs == null || _deadlineExceededCompleteTcs.Task.IsCompletedSuccessfully))
        {
            // Fast-path to avoid async state machine.
            DisposeCore();
            return default;
        }

        return DeadlineDisposeAsyncCore(disposeTask);
    }

    private async ValueTask DeadlineDisposeAsyncCore(ValueTask disposeTask)
    {
        await disposeTask;
        // Ensure an in-progress deadline is finished before disposing.
        // Need to await to avoid race between canceling CT and disposing it.
        if (_deadlineExceededCompleteTcs != null)
        {
            await _deadlineExceededCompleteTcs.Task;
        }

        DisposeCore();
    }

    private void DisposeCore()
    {
        // Remove request abort registration before disposing _deadlineCts.
        // Don't want an aborted request to attempt to cancel a disposed CTS.
        _requestAbortedRegistration.Dispose();

        _deadlineCts?.Dispose();
    }
}