using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Filters;
using Silky.Rpc.Transport;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client;

internal abstract partial class RemoteInvokerBase : IRemoteInvoker
{
    protected readonly ILogger _logger;
    protected readonly ClientInvokeContext _clientInvokeContext;
    protected readonly IClientInvokeContextAccessor _clientInvokeContextAccessor;
    protected readonly IClientFilterMetadata[] _filters;
    protected readonly IClientInvokeDiagnosticListener _clientInvokeDiagnosticListener;
    protected readonly ITransportClient _client;
    protected FilterCursor _cursor;

    protected RemoteResultMessage _result;

    private ClientInvokeExceptionContextSealed? _exceptionContext;
    private ClientResultExecutingContextSealed? _resultExecutingContext;
    private ClientResultExecutedContextSealed? _resultExecutedContext;
    private ClientAuthorizationFilterContextSealed? _authorizationContext;

    public RemoteResultMessage RemoteResult => _result!;

    protected RemoteInvokerBase(ILogger logger,
        ClientInvokeContext clientInvokeContext,
        IClientInvokeContextAccessor clientInvokeContextAccessor,
        IClientInvokeDiagnosticListener clientInvokeDiagnosticListener,
        ITransportClient client,
        IClientFilterMetadata[] filters)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clientInvokeContext = clientInvokeContext ?? throw new ArgumentNullException(nameof(clientInvokeContext));
        _clientInvokeContextAccessor = clientInvokeContextAccessor ??
                                       throw new ArgumentNullException(nameof(clientInvokeContextAccessor));
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        _clientInvokeDiagnosticListener = clientInvokeDiagnosticListener ??
                                          throw new ArgumentNullException(nameof(clientInvokeDiagnosticListener));
        _client = client ?? throw new ArgumentNullException(nameof(client));

        _cursor = new FilterCursor(filters);
    }

    public virtual Task InvokeAsync()
    {
        _clientInvokeContextAccessor.ClientInvokeContext = _clientInvokeContext;
        var scope = _logger.ActionScope(_clientInvokeContext.RemoteInvokeMessage);
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

        static async Task Awaited(RemoteInvokerBase invoker, Task task, IDisposable? scope)
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

        static async Task Awaited(RemoteInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
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
                var current = _cursor.GetNextFilter<IClientAuthorizationFilter, IAsyncClientAuthorizationFilter>();
                if (current.FilterAsync != null)
                {
                    if (_authorizationContext == null)
                    {
                        _authorizationContext =
                            new ClientAuthorizationFilterContextSealed(_clientInvokeContext, _filters);
                    }

                    state = current.FilterAsync;
                    goto case State.AuthorizationAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    if (_authorizationContext == null)
                    {
                        _authorizationContext =
                            new ClientAuthorizationFilterContextSealed(_clientInvokeContext, _filters);
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
                var filter = (IAsyncClientAuthorizationFilter)state;
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

                var filter = (IAsyncClientAuthorizationFilter)state;
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

                var filter = (IClientAuthorizationFilter)state;
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
                var current = _cursor.GetNextFilter<IClientExceptionFilter, IAsyncClientExceptionFilter>();
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

                var filter = (IAsyncClientExceptionFilter)state;
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

                var filter = (IAsyncClientExceptionFilter)state;
                var exceptionContext = _exceptionContext;

                // _diagnosticListener.AfterOnExceptionAsync(exceptionContext, filter);
                // _logger.AfterExecutingMethodOnFilter(
                //     FilterTypeConstants.ExceptionFilter,
                //     nameof(IAsyncClientExceptionFilter.OnExceptionAsync),
                //     filter);
                //
                // if (exceptionContext.Exception == null || exceptionContext.ExceptionHandled)
                // {
                //     // We don't need to do anything to trigger a short circuit. If there's another
                //     // exception filter on the stack it will check the same set of conditions
                //     // and then just skip itself.
                //     _logger.ExceptionFilterShortCircuited(filter);
                // }

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

                var filter = (IClientExceptionFilter)state;
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
                    ResultNext<IAlwaysRunClientResultFilter, IAsyncAlwaysRunClientResultFilter>(ref next, ref scope,
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

        static async Task Awaited(RemoteInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
            bool isCompleted)
        {
            await lastTask;

            while (!isCompleted)
            {
                await invoker.ResultNext<IAlwaysRunClientResultFilter, IAsyncAlwaysRunClientResultFilter>(ref next,
                    ref scope, ref state, ref isCompleted);
            }
        }
    }
    
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

        static async Task Awaited(RemoteInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
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
                    new ClientInvokeExceptionContextSealed(invoker._clientInvokeContext, invoker._filters)
                    {
                        ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                    };
            }
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
                    ResultNext<IClientResultFilter, IAsyncClientResultFilter>(ref next, ref scope, ref state,
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

        static async Task Awaited(RemoteInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
            bool isCompleted)
        {
            await lastTask;

            while (!isCompleted)
            {
                await invoker.ResultNext<IClientResultFilter, IAsyncClientResultFilter>(ref next, ref scope, ref state,
                    ref isCompleted);
            }
        }
    }
    
    protected abstract Task InvokeInnerFilterAsync();

    private Task ResultNext<TFilter, TFilterAsync>(ref State next, ref Scope scope, ref object? state,
        ref bool isCompleted)
        where TFilter : class, IClientResultFilter
        where TFilterAsync : class, IAsyncClientResultFilter
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
                            new ClientResultExecutingContextSealed(_clientInvokeContext, _filters, _result!);
                    }

                    state = current.FilterAsync;
                    goto case State.ResultAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    if (_resultExecutingContext == null)
                    {
                        _resultExecutingContext =
                            new ClientResultExecutingContextSealed(_clientInvokeContext, _filters, _result!);
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
                    _resultExecutedContext = new ClientResultExecutedContextSealed(
                        _clientInvokeContext,
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
                    _resultExecutedContext = new ClientResultExecutedContextSealed(
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
                    _result = new InvokeRemoteEmptyResult(_clientInvokeContext);
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
                            new ClientResultExecutedContextSealed(_clientInvokeContext, _filters, result!);
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


    private Task<ClientResultExecutedContext> InvokeNextResultFilterAwaitedAsync<TFilter, TFilterAsync>()
        where TFilter : class, IClientResultFilter
        where TFilterAsync : class, IAsyncClientResultFilter
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
        return Task.FromResult<ClientResultExecutedContext>(_resultExecutedContext);
        static async Task<ClientResultExecutedContext> Awaited(RemoteInvokerBase invoker, Task task)
        {
            await task;

            Debug.Assert(invoker._resultExecutedContext != null);
            return invoker._resultExecutedContext;
        }
#pragma warning disable CS1998
        static async Task<ClientResultExecutedContext> Throw()
        {
            throw new InvalidOperationException($"InvokeAsync {typeof(IAsyncClientResultFilter).Name} Fail");
        }
#pragma warning restore CS1998
    }

    private Task InvokeNextResultFilterAsync<TFilter, TFilterAsync>()
        where TFilter : class, IClientResultFilter
        where TFilterAsync : class, IAsyncClientResultFilter
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
            _resultExecutedContext =
                new ClientResultExecutedContextSealed(_clientInvokeContext, _filters, _result!)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                };
        }

        Debug.Assert(_resultExecutedContext != null);

        return Task.CompletedTask;

        static async Task Awaited(RemoteInvokerBase invoker, Task lastTask, State next, Scope scope, object? state,
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
                invoker._resultExecutedContext = new ClientResultExecutedContextSealed(invoker._clientInvokeContext,
                    invoker._filters, invoker._result!)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                };
            }

            Debug.Assert(invoker._resultExecutedContext != null);
        }
    }
    
    private static void Rethrow(ClientResultExecutedContextSealed context)
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
    
    private static void Rethrow(ClientInvokeExceptionContextSealed context)
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
        public const string AuthorizationFilter = "Client Authorization Filter";
        public const string ResourceFilter = "Client Resource Filter";
        public const string ActionFilter = "Client Action Filter";
        public const string ExceptionFilter = "Client Exception Filter";
        public const string ResultFilter = "Client RemoteResult Filter";
        public const string AlwaysRunResultFilter = "Client Always Run RemoteResult Filter";
    }

    private sealed class ClientInvokeExceptionContextSealed : ClientInvokeExceptionContext
    {
        public ClientInvokeExceptionContextSealed(ClientInvokeContext context, IList<IFilterMetadata> filters) : base(
            context, filters)
        {
        }
    }

    private sealed class ClientResultExecutingContextSealed : ClientResultExecutingContext
    {
        public ClientResultExecutingContextSealed(
            ClientInvokeContext context,
            IList<IFilterMetadata> filters,
            RemoteResultMessage result)
            : base(context, filters, result)
        {
        }
    }

    private sealed class ClientResultExecutedContextSealed : ClientResultExecutedContext
    {
        public ClientResultExecutedContextSealed(
            ClientInvokeContext context,
            IList<IFilterMetadata> filters,
            RemoteResultMessage result)
            : base(context, filters, result) // todo set RemoteResult
        {
        }
    }

    private sealed class ClientAuthorizationFilterContextSealed : ClientAuthorizationFilterContext
    {
        public ClientAuthorizationFilterContextSealed(ClientInvokeContext context,
            IList<IFilterMetadata> filters) :
            base(context, filters)
        {
        }
    }
}