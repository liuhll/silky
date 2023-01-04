using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Rpc.Filters;
using IFilterMetadata = Silky.Rpc.Filters.IFilterMetadata;
using ResultExecutedContext = Silky.Rpc.Filters.ResultExecutedContext;
using ResultExecutingContext = Silky.Rpc.Filters.ResultExecutingContext;


namespace Silky.Rpc.Runtime.Server;

public abstract partial class LocalInvoker
{
    protected readonly ILogger _logger;
    protected readonly ServiceEntryContext _serviceEntryContext;
    protected readonly IServerFilterMetadata[] _filters;
    protected readonly IServiceEntryContextAccessor _serviceEntryContextAccessor;

    protected object _result;

    // Do not make this readonly, it's mutable. We don't want to make a copy.
    // https://blogs.msdn.microsoft.com/ericlippert/2008/05/14/mutating-readonly-structs/
    protected FilterCursor _cursor;
    protected object? _instance;


    private ExceptionContextSealed? _exceptionContext;
    private ResultExecutingContextSealed? _resultExecutingContext;
    private ResultExecutedContextSealed? _resultExecutedContext;

    protected LocalInvoker(ILogger logger,
        ServiceEntryContext serviceEntryContext,
        IServiceEntryContextAccessor serviceEntryContextAccessor,
        IServerFilterMetadata[] filters)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceEntryContext = serviceEntryContext ?? throw new ArgumentNullException(nameof(serviceEntryContext));
        _serviceEntryContextAccessor = serviceEntryContextAccessor ??
                                       throw new ArgumentNullException(nameof(_serviceEntryContextAccessor));
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        _logger = logger;
        _serviceEntryContext = serviceEntryContext;
        _filters = filters;

        _cursor = new FilterCursor(filters);
    }

    public virtual Task InvokeAsync()
    {
        _serviceEntryContextAccessor.ServiceEntryContext = _serviceEntryContext;
        var scope = _logger.ActionScope(_serviceEntryContext.ServiceEntry);
        Task task;
        try
        {
            task = InvokeFilterPipelineAsync();
        }
        catch (Exception exception)
        {
            return Awaited(this, Task.FromException(exception), scope);
        }

        if (!task.IsCompletedSuccessfully)
        {
            return Awaited(this, task, scope);
        }

        return ReleaseResourcesCore(scope).AsTask();
        
        static async Task Awaited(LocalInvoker invoker, Task task, IDisposable? scope)
        {
            try
            {
                await task;
            }
            finally
            {
                await invoker.ReleaseResourcesCore(scope);
            }
        }

    }

    internal ValueTask ReleaseResourcesCore(IDisposable? scope)
    {
        Exception? releaseException = null;
        ValueTask releaseResult;
        try
        {
            releaseResult = ReleaseResources();
            if (!releaseResult.IsCompletedSuccessfully)
            {
                return HandleAsyncReleaseErrors(releaseResult, scope);
            }
        }
        catch (Exception exception)
        {
            releaseException = exception;
        }

        return HandleReleaseErrors(scope, releaseException);

        static async ValueTask HandleAsyncReleaseErrors(ValueTask releaseResult, IDisposable? scope)
        {
            Exception? releaseException = null;
            try
            {
                await releaseResult;
            }
            catch (Exception exception)
            {
                releaseException = exception;
            }

            await HandleReleaseErrors(scope, releaseException);
        }

        static ValueTask HandleReleaseErrors(IDisposable? scope, Exception? releaseException)
        {
            Exception? scopeException = null;
            try
            {
                scope?.Dispose();
            }
            catch (Exception exception)
            {
                scopeException = exception;
            }

            if (releaseException == null && scopeException == null)
            {
                return default;
            }
            else if (releaseException != null && scopeException != null)
            {
                return ValueTask.FromException(new AggregateException(releaseException, scopeException));
            }
            else if (releaseException != null)
            {
                return ValueTask.FromException(releaseException);
            }
            else
            {
                return ValueTask.FromException(scopeException!);
            }
        }
    }

    protected abstract ValueTask ReleaseResources();

    private Task InvokeFilterPipelineAsync()
    {
        var next = State.InvokeBegin;
        var scope = Scope.Invoker;
        var state = (object?)null;

        var isCompleted = false;

        try
        {
            while (!isCompleted)
            {
                var lastTask = Next(ref next, ref scope, ref state, ref isCompleted);
                if (!lastTask.IsCompletedSuccessfully)
                {
                    return Awaited(this, lastTask, next, scope, state, isCompleted);
                }
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Wrap non task-wrapped exceptions in a Task,
            // as this isn't done automatically since the method is not async.
            return Task.FromException(ex);
        }
        static async Task Awaited(LocalInvoker invoker, Task lastTask, State next, Scope scope, object? state, bool isCompleted)
        {
            await lastTask;

            while (!isCompleted)
            {
                await invoker.Next(ref next, ref scope, ref state, ref isCompleted);
            }
        }
    }

    private Task Next(ref State next, ref Scope scope, ref object? state, ref bool isCompleted)
    {
        switch (next)
        {
            case State.InvokeBegin:
            {
                goto case State.ExceptionBegin;
            }
            case State.ExceptionBegin:
            {
                _cursor.Reset();
                goto case State.ExceptionNext;
            }
            case State.ExceptionNext:
            {
                var current = _cursor.GetNextFilter<IExceptionServerFilter, IAsyncExceptionServerFilter>();
                if (current.FilterAsync != null)
                {
                    state = current.FilterAsync;
                    goto case State.ExceptionAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    state = current.Filter;
                    goto case State.ExceptionSyncBegin;
                }
                else if (scope == Scope.Exception)
                {
                    // All exception filters are on the stack already - so execute the 'inside'.
                    goto case State.ExceptionInside;
                }
                else
                {
                    // There are no exception filters - so jump right to the action.
                    Debug.Assert(scope == Scope.Invoker);
                    goto case State.ActionBegin;
                }
            }
            case State.ExceptionAsyncBegin:
            {
                var task = InvokeNextExceptionFilterAsync();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.ExceptionAsyncResume;
                    return task;
                }

                goto case State.ExceptionAsyncResume;
            }
            case State.ExceptionAsyncResume:
            {
                Debug.Assert(state != null);

                var filter = (IAsyncExceptionServerFilter)state;
                var exceptionContext = _exceptionContext;

                // When we get here we're 'unwinding' the stack of exception filters. If we have an unhandled exception,
                // we'll call the filter. Otherwise there's nothing to do.
                if (exceptionContext?.Exception != null && !exceptionContext.ExceptionHandled)
                {
                    var task = filter.OnExceptionAsync(exceptionContext);
                    if (!task.IsCompletedSuccessfully)
                    {
                        next = State.ExceptionAsyncEnd;
                        return task;
                    }

                    goto case State.ExceptionAsyncEnd;
                }

                goto case State.ExceptionEnd;
            }
            case State.ExceptionAsyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_exceptionContext != null);
                
                goto case State.ExceptionEnd;
            }
            case State.ExceptionSyncBegin:
            {
                var task = InvokeNextExceptionFilterAsync();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.ExceptionSyncEnd;
                    return task;
                }

                goto case State.ExceptionSyncEnd;
            }
            case State.ExceptionSyncEnd:
            {
                Debug.Assert(state != null);

                var filter = (IExceptionServerFilter)state;
                var exceptionContext = _exceptionContext;
                if (exceptionContext?.Exception != null && !exceptionContext.ExceptionHandled)
                {
                    filter.OnException(exceptionContext);
                }
                goto case State.ExceptionEnd;
            }
            case State.ExceptionInside:
            {
                goto case State.ActionBegin;
            }
            case State.ExceptionHandled:
            {
                Debug.Assert(state != null);
                Debug.Assert(_exceptionContext != null);

                if (_exceptionContext.Result == null)
                {
                    _exceptionContext.Result = default;
                }

                _result = _exceptionContext.Result;

                var task = InvokeAlwaysRunResultFilters();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.InvokeEnd;
                    return task;
                }

                goto case State.InvokeEnd;
            }
            case State.ExceptionEnd:
            {
                var exceptionContext = _exceptionContext;

                if (scope == Scope.Exception)
                {
                    isCompleted = true;
                    return Task.CompletedTask;
                }
                if (exceptionContext != null)
                {
                    if (exceptionContext.Result != null ||
                        exceptionContext.Exception == null ||
                        exceptionContext.ExceptionHandled)
                    {
                        goto case State.ExceptionHandled;
                    }

                    Rethrow(exceptionContext);
                    Debug.Fail("unreachable");
                }
                goto case State.InvokeEnd;
            }
            case State.ActionBegin:
            {
                var task = InvokeInnerFilterAsync();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.ActionEnd;
                    return task;
                }

                goto case State.ActionEnd;
            }
            case State.ActionEnd:
            {
                if (scope == Scope.Exception)
                {
                    // If we're inside an exception filter, let's allow those filters to 'unwind' before
                    // the result.
                    isCompleted = true;
                    return Task.CompletedTask;
                }

                Debug.Assert(scope == Scope.Invoker);
                var task = InvokeResultFilters();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.InvokeEnd;
                    return task;
                }
                goto case State.InvokeEnd;
            }
            case State.InvokeEnd:
            {
                isCompleted = true;
                return Task.CompletedTask;
            }
            default:
                throw new InvalidOperationException();
        }
    }

    private Task InvokeAlwaysRunResultFilters()
    {
        throw new NotImplementedException();
    }

    private static void Rethrow(ExceptionContextSealed context)
    {
        if (context == null)
        {
            return;
        }

        if (context.ExceptionHandled)
        {
            return;
        }

        if (context.ExceptionDispatchInfo != null)
        {
            context.ExceptionDispatchInfo.Throw();
        }

        if (context.Exception != null)
        {
            throw context.Exception;
        }
    }

    private Task InvokeResultFilters()
    {
        throw new NotImplementedException();
    }

    protected abstract Task InvokeInnerFilterAsync();
    private Task InvokeNextExceptionFilterAsync()
    {
        return Task.CompletedTask;
    }

    public object Result => _result;

    private enum State
    {
        InvokeBegin,

        // AuthorizationBegin,
        // AuthorizationNext,
        // AuthorizationAsyncBegin,
        // AuthorizationAsyncEnd,
        // AuthorizationSync,
        // AuthorizationShortCircuit,
        // AuthorizationEnd,
        // ResourceBegin,
        // ResourceNext,
        // ResourceAsyncBegin,
        // ResourceAsyncEnd,
        // ResourceSyncBegin,
        // ResourceSyncEnd,
        // ResourceShortCircuit,
        // ResourceInside,
        // ResourceInsideEnd,
        // ResourceEnd,
        ExceptionBegin,
        ExceptionNext,
        ExceptionAsyncBegin,
        ExceptionAsyncResume,
        ExceptionAsyncEnd,
        ExceptionSyncBegin,
        ExceptionSyncEnd,
        ExceptionInside,
        ExceptionHandled,
        ExceptionEnd,
        ActionBegin,
        ActionEnd,
        ResultBegin,
        ResultNext,
        ResultAsyncBegin,
        ResultAsyncEnd,
        ResultSyncBegin,
        ResultSyncEnd,
        ResultInside,
        ResultEnd,
        InvokeEnd,
    }

    private enum Scope
    {
        Invoker,

        // Resource,
        Exception,
        Result,
    }

    private sealed class ExceptionContextSealed : ServerExceptionContext
    {
        public ExceptionContextSealed(ServiceEntryContext serviceEntryContext, IList<IFilterMetadata> filters) : base(
            serviceEntryContext, filters)
        {
        }
    }

    private sealed class ResultExecutedContextSealed : ResultExecutedContext
    {
        public ResultExecutedContextSealed(
            ServiceEntryContext serviceEntryContext,
            IList<IFilterMetadata> filters,
            object result)
            : base(serviceEntryContext, filters, result)
        {
        }
    }

    private sealed class ResultExecutingContextSealed : ResultExecutingContext
    {
        public ResultExecutingContextSealed(
            ServiceEntryContext serviceEntryContext,
            IList<IFilterMetadata> filters,
            object result)
            : base(serviceEntryContext, filters, result)
        {
        }
    }
}