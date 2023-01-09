using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;
using IFilterMetadata = Silky.Core.FilterMetadata.IFilterMetadata;


namespace Silky.Rpc.Runtime.Server;

public abstract partial class LocalInvokerBase : ILocalInvoker
{
    protected readonly ILogger _logger;
    protected readonly ServiceEntryContext _serviceEntryContext;
    protected readonly IServerFilterMetadata[] _filters;
    protected readonly IServiceEntryContextAccessor _serviceEntryContextAccessor;

    protected object _result;

    // Do not make this readonly, it's mutable. We don't want to make a copy.
    // https://blogs.msdn.microsoft.com/ericlippert/2008/05/14/mutating-readonly-structs/
    protected FilterCursor _cursor;

    private ServerInvokeExceptionContextSealed? _exceptionContext;
    private ServerResultExecutingContextSealed? _resultExecutingContext;
    private ServerResultExecutedContextSealed? _resultExecutedContext;
    private ServerAuthorizationFilterContextSealed? _authorizationContext;

    protected LocalInvokerBase(ILogger logger,
        ServiceEntryContext serviceEntryContext,
        IServiceEntryContextAccessor serviceEntryContextAccessor,
        IServerFilterMetadata[] filters)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceEntryContext = serviceEntryContext ?? throw new ArgumentNullException(nameof(serviceEntryContext));
        _serviceEntryContextAccessor = serviceEntryContextAccessor ??
                                       throw new ArgumentNullException(nameof(_serviceEntryContextAccessor));
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));

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

        static async Task Awaited(LocalInvokerBase invoker, Task task, IDisposable? scope)
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

        static async Task Awaited(LocalInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
            bool isCompleted)
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
                goto case State.AuthorizationBegin;
            }
            case State.AuthorizationBegin:
            {
                _cursor.Reset();
                goto case State.AuthorizationNext;
            }
            case State.AuthorizationNext:
            {
                var current = _cursor.GetNextFilter<IServerAuthorizationFilter, IAsyncServerAuthorizationFilter>();
                if (current.FilterAsync != null)
                {
                    if (_authorizationContext == null)
                    {
                        _authorizationContext =
                            new ServerAuthorizationFilterContextSealed(_serviceEntryContext, _filters);
                    }

                    state = current.FilterAsync;
                    goto case State.AuthorizationAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    if (_authorizationContext == null)
                    {
                        _authorizationContext =
                            new ServerAuthorizationFilterContextSealed(_serviceEntryContext, _filters);
                    }

                    state = current.Filter;
                    goto case State.AuthorizationSync;
                }
                else
                {
                    goto case State.AuthorizationEnd;
                }
            }
            case State.AuthorizationAsyncBegin:
            {
                Debug.Assert(state != null);
                Debug.Assert(_authorizationContext != null);

                var filter = (IAsyncServerAuthorizationFilter)state;
                var authorizationContext = _authorizationContext;
                var task = filter.OnAuthorizationAsync(authorizationContext);
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.AuthorizationAsyncEnd;
                    return task;
                }

                goto case State.AuthorizationAsyncEnd;
            }
            case State.AuthorizationAsyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_authorizationContext != null);

                var filter = (IAsyncServerAuthorizationFilter)state;
                var authorizationContext = _authorizationContext;
                if (authorizationContext.Result != null)
                {
                    goto case State.AuthorizationShortCircuit;
                }

                goto case State.AuthorizationNext;
            }
            case State.AuthorizationSync:
            {
                Debug.Assert(state != null);
                Debug.Assert(_authorizationContext != null);

                var filter = (IServerAuthorizationFilter)state;
                var authorizationContext = _authorizationContext;

                filter.OnAuthorization(authorizationContext);

                if (authorizationContext.Result != null)
                {
                    goto case State.AuthorizationShortCircuit;
                }

                goto case State.AuthorizationNext;
            }
            case State.AuthorizationShortCircuit:
            {
                Debug.Assert(state != null);
                Debug.Assert(_authorizationContext != null);
                Debug.Assert(_authorizationContext.Result != null);

                isCompleted = true;
                _result = _authorizationContext.Result;
                return InvokeAlwaysRunResultFilters();
            }
            case State.AuthorizationEnd:
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
                var current = _cursor.GetNextFilter<IServerExceptionFilter, IAsyncServerExceptionFilter>();
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

                var filter = (IAsyncServerExceptionFilter)state;
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

                var filter = (IServerExceptionFilter)state;
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

                var task = InvokeResultFilters();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.InvokeEnd;
                    return task;
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
        try
        {
            var next = State.ResultBegin;
            var scope = Scope.Invoker;
            var state = (object?)null;
            var isCompleted = false;

            while (!isCompleted)
            {
                var lastTask =
                    ResultNext<IAlwaysRunServerResultFilter, IAsyncAlwaysRunServerResultFilter>(ref next, ref scope,
                        ref state, ref isCompleted);
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

        static async Task Awaited(LocalInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
            bool isCompleted)
        {
            await lastTask;

            while (!isCompleted)
            {
                await invoker.ResultNext<IAlwaysRunServerResultFilter, IAsyncAlwaysRunServerResultFilter>(ref next,
                    ref scope, ref state, ref isCompleted);
            }
        }
    }

    private Task ResultNext<TFilter, TFilterAsync>(ref State next, ref Scope scope, ref object? state,
        ref bool isCompleted)
        where TFilter : class, IServerResultFilter
        where TFilterAsync : class, IAsyncServerResultFilter
    {
        var resultFilterKind = typeof(TFilter) == typeof(IAlwaysRunServerResultFilter)
            ? FilterTypeConstants.AlwaysRunResultFilter
            : FilterTypeConstants.ResultFilter;
        switch (next)
        {
            case State.ResultBegin:
            {
                _cursor.Reset();
                goto case State.ResultNext;
            }
            case State.ResultNext:
            {
                var current = _cursor.GetNextFilter<TFilter, TFilterAsync>();
                if (current.FilterAsync != null)
                {
                    if (_resultExecutingContext == null)
                    {
                        _resultExecutingContext =
                            new ServerResultExecutingContextSealed(_serviceEntryContext, _filters, _result!);
                    }

                    state = current.FilterAsync;
                    goto case State.ResultAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    if (_resultExecutingContext == null)
                    {
                        _resultExecutingContext =
                            new ServerResultExecutingContextSealed(_serviceEntryContext, _filters, _result!);
                    }

                    state = current.Filter;
                    goto case State.ResultSyncBegin;
                }
                else
                {
                    goto case State.ResultInside;
                }
            }
            case State.ResultAsyncBegin:
            {
                Debug.Assert(state != null);
                Debug.Assert(_resultExecutingContext != null);
                var filter = (TFilterAsync)state;
                var resultExecutingContext = _resultExecutingContext;

                var task = filter.OnResultExecutionAsync(resultExecutingContext,
                    InvokeNextResultFilterAwaitedAsync<TFilter, TFilterAsync>);
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.ResultAsyncEnd;
                    return task;
                }

                goto case State.ResultAsyncEnd;
            }
            case State.ResultAsyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_resultExecutingContext != null);
                var filter = (TFilterAsync)state;
                var resultExecutingContext = _resultExecutingContext;
                var resultExecutedContext = _resultExecutedContext;
                if (resultExecutedContext == null || resultExecutingContext.Cancel)
                {
                    // Short-circuited by not calling next || Short-circuited by setting Cancel == true
                    _resultExecutedContext = new ServerResultExecutedContextSealed(
                        _serviceEntryContext,
                        _filters,
                        resultExecutingContext.Result)
                    {
                        Canceled = true,
                    };
                }

                goto case State.ResultEnd;
            }

            case State.ResultSyncBegin:
            {
                Debug.Assert(state != null);
                Debug.Assert(_resultExecutingContext != null);

                var filter = (TFilter)state;
                var resultExecutingContext = _resultExecutingContext;
                filter.OnResultExecuting(resultExecutingContext);
                if (_resultExecutingContext.Cancel)
                {
                    _resultExecutedContext = new ServerResultExecutedContextSealed(
                        resultExecutingContext,
                        _filters,
                        resultExecutingContext.Result)
                    {
                        Canceled = true,
                    };

                    goto case State.ResultEnd;
                }

                var task = InvokeNextResultFilterAsync<TFilter, TFilterAsync>();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.ResultSyncEnd;
                    return task;
                }

                goto case State.ResultSyncEnd;
            }
            case State.ResultSyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_resultExecutingContext != null);
                Debug.Assert(_resultExecutedContext != null);
                var filter = (TFilter)state;
                var resultExecutedContext = _resultExecutedContext;
                filter.OnResultExecuted(resultExecutedContext);
                goto case State.ResultEnd;
            }
            case State.ResultInside:
            {
                if (_resultExecutingContext != null)
                {
                    _result = _resultExecutingContext.Result;
                }

                if (_result == null)
                {
                    // The empty result is always flowed back as the 'executed' result if we don't have one.
                    _result = default;
                }

                goto case State.ResultEnd;
            }

            case State.ResultEnd:
            {
                var result = _result;
                isCompleted = true;

                if (scope == Scope.Result)
                {
                    if (_resultExecutedContext == null)
                    {
                        _resultExecutedContext =
                            new ServerResultExecutedContextSealed(_serviceEntryContext, _filters, result!);
                    }

                    return Task.CompletedTask;
                }

                Rethrow(_resultExecutedContext!);
                return Task.CompletedTask;
            }
            default:
                throw new InvalidOperationException(); // Unreachable.
        }
    }


    private Task<ServerResultExecutedContext> InvokeNextResultFilterAwaitedAsync<TFilter, TFilterAsync>()
        where TFilter : class, IServerResultFilter
        where TFilterAsync : class, IAsyncServerResultFilter
    {
        Debug.Assert(_resultExecutingContext != null);
        if (_resultExecutingContext.Cancel)
        {
            // If we get here, it means that an async filter set cancel == true AND called next().
            // This is forbidden.
            return Throw();
        }

        var task = InvokeNextResultFilterAsync<TFilter, TFilterAsync>();
        if (!task.IsCompletedSuccessfully)
        {
            return Awaited(this, task);
        }

        Debug.Assert(_resultExecutedContext != null);
        return Task.FromResult<ServerResultExecutedContext>(_resultExecutedContext);
        static async Task<ServerResultExecutedContext> Awaited(LocalInvokerBase invoker, Task task)
        {
            await task;

            Debug.Assert(invoker._resultExecutedContext != null);
            return invoker._resultExecutedContext;
        }
#pragma warning disable CS1998
        static async Task<ServerResultExecutedContext> Throw()
        {
            throw new InvalidOperationException($"InvokeAsync {typeof(IAsyncServerResultFilter).Name} Fail");
        }
#pragma warning restore CS1998
    }

    private Task InvokeNextResultFilterAsync<TFilter, TFilterAsync>()
        where TFilter : class, IServerResultFilter
        where TFilterAsync : class, IAsyncServerResultFilter
    {
        try
        {
            var next = State.ResultNext;
            var state = (object?)null;
            var scope = Scope.Result;
            var isCompleted = false;
            while (!isCompleted)
            {
                var lastTask = ResultNext<TFilter, TFilterAsync>(ref next, ref scope, ref state, ref isCompleted);
                if (!lastTask.IsCompletedSuccessfully)
                {
                    return Awaited(this, lastTask, next, scope, state, isCompleted);
                }
            }
        }
        catch (Exception exception)
        {
            _resultExecutedContext = new ServerResultExecutedContextSealed(_serviceEntryContext, _filters, _result!)
            {
                ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
            };
        }

        Debug.Assert(_resultExecutedContext != null);

        return Task.CompletedTask;

        static async Task Awaited(LocalInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
            bool isCompleted)
        {
            try
            {
                await lastTask;

                while (!isCompleted)
                {
                    await invoker.ResultNext<TFilter, TFilterAsync>(ref next, ref scope, ref state, ref isCompleted);
                }
            }
            catch (Exception exception)
            {
                invoker._resultExecutedContext = new ServerResultExecutedContextSealed(invoker._serviceEntryContext,
                    invoker._filters, invoker._result!)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                };
            }

            Debug.Assert(invoker._resultExecutedContext != null);
        }
    }

    private static void Rethrow(ServerInvokeExceptionContextSealed context)
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

    private static void Rethrow(ServerResultExecutedContextSealed context)
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
        try
        {
            var next = State.ResultBegin;
            var scope = Scope.Invoker;
            var state = (object?)null;
            var isCompleted = false;

            while (!isCompleted)
            {
                var lastTask =
                    ResultNext<IServerResultFilter, IAsyncServerResultFilter>(ref next, ref scope, ref state,
                        ref isCompleted);
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

        static async Task Awaited(LocalInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
            bool isCompleted)
        {
            await lastTask;

            while (!isCompleted)
            {
                await invoker.ResultNext<IServerResultFilter, IAsyncServerResultFilter>(ref next, ref scope, ref state,
                    ref isCompleted);
            }
        }
    }

    protected abstract Task InvokeInnerFilterAsync();

    private Task InvokeNextExceptionFilterAsync()
    {
        try
        {
            var next = State.ExceptionNext;
            var state = (object?)null;
            var scope = Scope.Exception;
            var isCompleted = false;

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

        static async Task Awaited(LocalInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
            bool isCompleted)
        {
            try
            {
                await lastTask;

                while (!isCompleted)
                {
                    await invoker.Next(ref next, ref scope, ref state, ref isCompleted);
                }
            }
            catch (Exception exception)
            {
                invoker._exceptionContext =
                    new ServerInvokeExceptionContextSealed(invoker._serviceEntryContext, invoker._filters)
                    {
                        ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                    };
            }
        }
    }

    public object Result => _result;

    private enum State
    {
        InvokeBegin,
        AuthorizationBegin,
        AuthorizationNext,
        AuthorizationAsyncBegin,
        AuthorizationAsyncEnd,
        AuthorizationSync,
        AuthorizationShortCircuit,
        AuthorizationEnd,
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

    private static class FilterTypeConstants
    {
        public const string AuthorizationFilter = "Server Authorization Filter";

        // public const string ResourceFilter = "Resource Filter";
        public const string ActionFilter = "Server Action Filter";
        public const string ExceptionFilter = "Server Exception Filter";
        public const string ResultFilter = "Server RemoteResult Filter";
        public const string AlwaysRunResultFilter = "Server  Always Run RemoteResult Filter";
    }

    private sealed class ServerInvokeExceptionContextSealed : ServerInvokeExceptionContext
    {
        public ServerInvokeExceptionContextSealed(ServiceEntryContext serviceEntryContext,
            IList<IFilterMetadata> filters) : base(
            serviceEntryContext, filters)
        {
        }
    }

    private sealed class ServerResultExecutedContextSealed : ServerResultExecutedContext
    {
        public ServerResultExecutedContextSealed(
            ServiceEntryContext serviceEntryContext,
            IList<IFilterMetadata> filters,
            object result)
            : base(serviceEntryContext, filters, result)
        {
        }
    }

    private sealed class ServerAuthorizationFilterContextSealed : ServerAuthorizationFilterContext
    {
        public ServerAuthorizationFilterContextSealed(ServiceEntryContext context,
            IList<IFilterMetadata> filters) : base(context, filters)
        {
        }
    }

    private sealed class ServerResultExecutingContextSealed : ServerResultExecutingContext
    {
        public ServerResultExecutingContextSealed(
            ServiceEntryContext serviceEntryContext,
            IList<IFilterMetadata> filters,
            object result)
            : base(serviceEntryContext, filters, result)
        {
        }
    }
}