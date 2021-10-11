using System;
using System.Threading;
using System.Threading.Tasks;

namespace Silky.Core.Extensions
{
    public static class TaskCompletionSourceExtensions
    {
        /// <summary>
        /// This allows a TaskCompletionSourceExtensions to be await with a cancellation token and timeout.
        /// 
        /// Example usable:
        /// 
        ///     var tcs = new TaskCompletionSourceExtensions<bool>();
        ///           ...
        ///     var result = await tcs.WaitAsync(timeout);
        /// 
        /// A TaskCanceledException will be thrown if the given cancelToken is canceled before the tcs completes or errors. 
        /// </summary>
        /// <typeparam name="TResult">Result type of the TaskCompletionSourceExtensions</typeparam>
        /// <param name="tcs">The task completion source to be used  </param>
        /// <param name="cancelToken">This method will throw an OperationCanceledException if the cancelToken is canceled</param>
        /// <param name="timeoutMs">This method will throw a TimeoutException if it doesn't complete within the given timeout, unless the timeout is less then or equal to 0 or Timeout.Infinite</param>
        /// <returns>The tcs.Task</returns>
        public static async Task<TResult> WaitAsync<TResult>(this TaskCompletionSource<TResult> tcs,
            CancellationToken cancelToken, int timeoutMs = Timeout.Infinite)
        {
            // The overrideTcs is used so we can wait for either the give tcs to complete or the overrideTcs.  We do this using the Task.WhenAny method.
            // one issue with WhenAny is that it won't return when a task is canceled, it only returns when a task completes so we complete the
            // overrideTcs when either the cancelToken is canceled or the timeoutMs is reached.
            //
            var overrideTcs = new TaskCompletionSource<TResult>();
            using (var timeoutCancelTokenSource = (timeoutMs <= 0) ? null : new CancellationTokenSource(timeoutMs))
            {
                var timeoutToken = timeoutCancelTokenSource?.Token ?? CancellationToken.None;
                using (var linkedTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(cancelToken, timeoutToken))
                {
                    // This method is called when either the linkedTokenSource is canceled.  This lets us assign a value to the overrideTcs so that
                    // We can break out of the await WhenAny below.
                    //
                    void CancelTcs()
                    {
                        if (!tcs.Task.IsCompleted)
                        {
                            // ReSharper disable once AccessToDisposedClosure (in this case, CancelTcs will never be called outside the using)
                            if (timeoutCancelTokenSource?.IsCancellationRequested ?? false)
                                tcs.TrySetException(new TimeoutException($"WaitAsync timed out after {timeoutMs}ms"));
                            else
                                tcs.TrySetCanceled();
                        }

                        overrideTcs.TrySetResult(default(TResult));
                    }

                    using (linkedTokenSource.Token.Register(CancelTcs))
                    {
                        try
                        {
                            await Task.WhenAny(tcs.Task, overrideTcs.Task);
                        }
                        catch
                        {
                            /* ignore */
                        }

                        // We always favor the result from the given tcs task if it has completed.
                        //
                        if (tcs.Task.IsCompleted)
                        {
                            // We do another await here so that if the tcs.Task has faulted or has been canceled we won't wrap those exceptions
                            // in a nested exception.  While technically accessing the tcs.Task.Result will generate the same exception the
                            // exception will be wrapped in a nested exception.  We don't want that nesting so we just await.
                            await tcs.Task;
                            return tcs.Task.Result;
                        }

                        // It wasn't the tcs.Task that got us our of the above WhenAny so go ahead and timeout or cancel the operation.
                        //
                        if (timeoutCancelTokenSource?.IsCancellationRequested ?? false)
                            throw new TimeoutException($"WaitAsync timed out after {timeoutMs}ms");

                        throw new OperationCanceledException();
                    }
                }
            }
        }

        public static async Task<TResult> WaitAsync<TResult>(this TaskCompletionSource<TResult> tcs,
            int timeoutMs = Timeout.Infinite)
        {
            var overrideTcs = new TaskCompletionSource<TResult>();
            using (var timeoutCancelTokenSource = (timeoutMs <= 0) ? null : new CancellationTokenSource(timeoutMs))
            {
                var timeoutToken = timeoutCancelTokenSource?.Token ?? CancellationToken.None;

                void CancelTcs()
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        // ReSharper disable once AccessToDisposedClosure (in this case, CancelTcs will never be called outside the using)
                        if (timeoutCancelTokenSource?.IsCancellationRequested ?? false)
                            tcs.TrySetException(new TimeoutException($"WaitAsync timed out after {timeoutMs}ms"));
                        else
                            tcs.TrySetCanceled();
                    }

                    overrideTcs.TrySetResult(default(TResult));
                }

                using (timeoutToken.Register(CancelTcs))
                {
                    try
                    {
                        await Task.WhenAny(tcs.Task, overrideTcs.Task);
                    }
                    catch
                    {
                        /* ignore */
                    }

                    if (tcs.Task.IsCompleted)
                    {
                        // We do another await here so that if the tcs.Task has faulted or has been canceled we won't wrap those exceptions
                        // in a nested exception.  While technically accessing the tcs.Task.Result will generate the same exception the
                        // exception will be wrapped in a nested exception.  We don't want that nesting so we just await.
                        await tcs.Task;
                        return tcs.Task.Result;
                    }

                    if (timeoutCancelTokenSource?.IsCancellationRequested ?? false)
                        throw new TimeoutException($"WaitAsync timed out after {timeoutMs}ms");

                    throw new OperationCanceledException();
                }
            }
        }
    }
}