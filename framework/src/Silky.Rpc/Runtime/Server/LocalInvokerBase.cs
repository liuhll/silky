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

        // 注意：此字段不可设为 readonly，因为其内部状态需要被修改（避免复制）
        protected FilterCursor _cursor;

        private ServerInvokeExceptionContextSealed? _exceptionContext;
        private ServerResultExecutingContextSealed? _resultExecutingContext;
        private ServerResultExecutedContextSealed? _resultExecutedContext;
        private ServerAuthorizationFilterContextSealed? _authorizationContext;

        public LocalInvokerBase(ILogger logger,
            ServiceEntryContext serviceEntryContext,
            IServiceEntryContextAccessor serviceEntryContextAccessor,
            IServerFilterMetadata[] filters)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceEntryContext = serviceEntryContext ?? throw new ArgumentNullException(nameof(serviceEntryContext));
            // 修复了此处参数名错误的问题
            _serviceEntryContextAccessor = serviceEntryContextAccessor ?? throw new ArgumentNullException(nameof(serviceEntryContextAccessor));
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
                return AwaitWithScope(this, Task.FromException(exception), scope);
            }

            if (!task.IsCompletedSuccessfully)
            {
                return AwaitWithScope(this, task, scope);
            }

            return ReleaseResourcesCore(scope).AsTask();
        }

        private static async Task AwaitWithScope(LocalInvokerBase invoker, Task task, IDisposable? scope)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            finally
            {
                await invoker.ReleaseResourcesCore(scope).ConfigureAwait(false);
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
        }

        private static async ValueTask HandleAsyncReleaseErrors(ValueTask releaseResult, IDisposable? scope)
        {
            Exception? releaseException = null;
            try
            {
                await releaseResult.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                releaseException = exception;
            }

            await HandleReleaseErrors(scope, releaseException).ConfigureAwait(false);
        }

        private static ValueTask HandleReleaseErrors(IDisposable? scope, Exception? releaseException)
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

        protected abstract ValueTask ReleaseResources();

        private Task InvokeFilterPipelineAsync()
        {
            var next = State.InvokeBegin;
            var scope = Scope.Invoker;
            object? state = null;
            var isCompleted = false;

            try
            {
                while (!isCompleted)
                {
                    var lastTask = Next(ref next, ref scope, ref state, ref isCompleted);
                    if (!lastTask.IsCompletedSuccessfully)
                    {
                        return AwaitPipelineContinuation(this, lastTask, next, scope, state, isCompleted);
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // 对非任务包装的异常进行包装
                return Task.FromException(ex);
            }
        }

        private static async Task AwaitPipelineContinuation(LocalInvokerBase invoker, Task lastTask, State next, Scope scope, object? state, bool isCompleted)
        {
            await lastTask.ConfigureAwait(false);
            while (!isCompleted)
            {
                await invoker.Next(ref next, ref scope, ref state, ref isCompleted).ConfigureAwait(false);
            }
        }

        private Task Next(ref State next, ref Scope scope, ref object? state, ref bool isCompleted)
        {
            switch (next)
            {
                case State.InvokeBegin:
                    goto case State.AuthorizationBegin;

                case State.AuthorizationBegin:
                    _cursor.Reset();
                    goto case State.AuthorizationNext;

                case State.AuthorizationNext:
                    {
                        var current = _cursor.GetNextFilter<IServerAuthorizationFilter, IAsyncServerAuthorizationFilter>();
                        if (current.FilterAsync != null)
                        {
                            _authorizationContext ??= new ServerAuthorizationFilterContextSealed(_serviceEntryContext, _filters);
                            state = current.FilterAsync;
                            goto case State.AuthorizationAsyncBegin;
                        }
                        else if (current.Filter != null)
                        {
                            _authorizationContext ??= new ServerAuthorizationFilterContextSealed(_serviceEntryContext, _filters);
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
                        var task = filter.OnAuthorizationAsync(_authorizationContext);
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
                        if (_authorizationContext.Result != null)
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
                        filter.OnAuthorization(_authorizationContext);
                        if (_authorizationContext.Result != null)
                        {
                            goto case State.AuthorizationShortCircuit;
                        }
                        goto case State.AuthorizationNext;
                    }

                case State.AuthorizationShortCircuit:
                    {
                        // 直接短路，跳转至 AlwaysRun 结果过滤器
                        isCompleted = true;
                        _result = _authorizationContext.Result;
                        return InvokeAlwaysRunResultFilters();
                    }

                case State.AuthorizationEnd:
                    goto case State.ExceptionBegin;

                case State.ExceptionBegin:
                    _cursor.Reset();
                    goto case State.ExceptionNext;

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
                            goto case State.ExceptionInside;
                        }
                        else
                        {
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
                        if (_exceptionContext?.Exception != null && !_exceptionContext.ExceptionHandled)
                        {
                            var task = filter.OnExceptionAsync(_exceptionContext);
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
                        if (_exceptionContext != null &&
                            _exceptionContext.Exception != null &&
                            !_exceptionContext.ExceptionHandled)
                        {
                            filter.OnException(_exceptionContext);
                        }
                        goto case State.ExceptionEnd;
                    }

                case State.ExceptionInside:
                    goto case State.ActionBegin;

                case State.ExceptionHandled:
                    {
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
                object? state = null;
                var isCompleted = false;

                while (!isCompleted)
                {
                    var lastTask = ResultNext<IAlwaysRunServerResultFilter, IAsyncAlwaysRunServerResultFilter>(ref next, ref scope, ref state, ref isCompleted);
                    if (!lastTask.IsCompletedSuccessfully)
                    {
                        return AwaitResultContinuation(this, lastTask, next, scope, state, isCompleted);
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        private static async Task AwaitResultContinuation(LocalInvokerBase invoker, Task lastTask, State next, Scope scope, object? state, bool isCompleted)
        {
            await lastTask.ConfigureAwait(false);
            while (!isCompleted)
            {
                await invoker.ResultNext<IAlwaysRunServerResultFilter, IAsyncAlwaysRunServerResultFilter>(ref next, ref scope, ref state, ref isCompleted).ConfigureAwait(false);
            }
        }

        private Task ResultNext<TFilter, TFilterAsync>(ref State next, ref Scope scope, ref object? state, ref bool isCompleted)
            where TFilter : class, IServerResultFilter
            where TFilterAsync : class, IAsyncServerResultFilter
        {
            switch (next)
            {
                case State.ResultBegin:
                    _cursor.Reset();
                    goto case State.ResultNext;

                case State.ResultNext:
                    {
                        var current = _cursor.GetNextFilter<TFilter, TFilterAsync>();
                        if (current.FilterAsync != null)
                        {
                            _resultExecutingContext ??= new ServerResultExecutingContextSealed(_serviceEntryContext, _filters, _result!);
                            state = current.FilterAsync;
                            goto case State.ResultAsyncBegin;
                        }
                        else if (current.Filter != null)
                        {
                            _resultExecutingContext ??= new ServerResultExecutingContextSealed(_serviceEntryContext, _filters, _result!);
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
                        var task = filter.OnResultExecutionAsync(_resultExecutingContext, InvokeNextResultFilterAwaitedAsync<TFilter, TFilterAsync>);
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
                        if (_resultExecutedContext == null || _resultExecutingContext.Cancel)
                        {
                            _resultExecutedContext = new ServerResultExecutedContextSealed(
                                _serviceEntryContext,
                                _filters,
                                _resultExecutingContext.Result)
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
                        filter.OnResultExecuting(_resultExecutingContext);
                        if (_resultExecutingContext.Cancel)
                        {
                            _resultExecutedContext = new ServerResultExecutedContextSealed(
                                _serviceEntryContext,
                                _filters,
                                _resultExecutingContext.Result)
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
                        filter.OnResultExecuted(_resultExecutedContext);
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
                            _result = default;
                        }
                        goto case State.ResultEnd;
                    }

                case State.ResultEnd:
                    {
                        isCompleted = true;
                        if (scope == Scope.Result)
                        {
                            _resultExecutedContext ??= new ServerResultExecutedContextSealed(_serviceEntryContext, _filters, _result!);
                            return Task.CompletedTask;
                        }
                        Rethrow(_resultExecutedContext!);
                        return Task.CompletedTask;
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        private Task<ServerResultExecutedContext> InvokeNextResultFilterAwaitedAsync<TFilter, TFilterAsync>()
            where TFilter : class, IServerResultFilter
            where TFilterAsync : class, IAsyncServerResultFilter
        {
            Debug.Assert(_resultExecutingContext != null);
            if (_resultExecutingContext.Cancel)
            {
                return ThrowInvalidOperation();
            }

            var task = InvokeNextResultFilterAsync<TFilter, TFilterAsync>();
            if (!task.IsCompletedSuccessfully)
            {
                return AwaitResultExecuted(this, task);
            }

            Debug.Assert(_resultExecutedContext != null);
            return Task.FromResult<ServerResultExecutedContext>(_resultExecutedContext);
        }

        private static async Task<ServerResultExecutedContext> AwaitResultExecuted(LocalInvokerBase invoker, Task task)
        {
            await task.ConfigureAwait(false);
            Debug.Assert(invoker._resultExecutedContext != null);
            return invoker._resultExecutedContext;
        }

        private static async Task<ServerResultExecutedContext> ThrowInvalidOperation()
        {
            throw new InvalidOperationException($"InvokeAsync {typeof(IAsyncServerResultFilter).Name} Fail");
        }

        private Task InvokeNextResultFilterAsync<TFilter, TFilterAsync>()
            where TFilter : class, IServerResultFilter
            where TFilterAsync : class, IAsyncServerResultFilter
        {
            try
            {
                var next = State.ResultNext;
                object? state = null;
                var scope = Scope.Result;
                var isCompleted = false;

                while (!isCompleted)
                {
                    var lastTask = ResultNext<TFilter, TFilterAsync>(ref next, ref scope, ref state, ref isCompleted);
                    if (!lastTask.IsCompletedSuccessfully)
                    {
                        return AwaitResultContinuation(this, lastTask, next, scope, state, isCompleted);
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
        }

        private Task InvokeResultFilters()
        {
            try
            {
                var next = State.ResultBegin;
                var scope = Scope.Invoker;
                object? state = null;
                var isCompleted = false;

                while (!isCompleted)
                {
                    var lastTask = ResultNext<IServerResultFilter, IAsyncServerResultFilter>(ref next, ref scope, ref state, ref isCompleted);
                    if (!lastTask.IsCompletedSuccessfully)
                    {
                        return AwaitResultContinuation(this, lastTask, next, scope, state, isCompleted);
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        protected abstract Task InvokeInnerFilterAsync();

        private Task InvokeNextExceptionFilterAsync()
        {
            try
            {
                var next = State.ExceptionNext;
                object? state = null;
                var scope = Scope.Exception;
                var isCompleted = false;

                while (!isCompleted)
                {
                    var lastTask = Next(ref next, ref scope, ref state, ref isCompleted);
                    if (!lastTask.IsCompletedSuccessfully)
                    {
                        return AwaitPipelineContinuation(this, lastTask, next, scope, state, isCompleted);
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        public object Result => _result;

        private static void Rethrow(ServerInvokeExceptionContextSealed context)
        {
            if (context == null || context.ExceptionHandled)
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
            if (context == null || context.ExceptionHandled)
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
            Exception,
            Result,
        }

        private static class FilterTypeConstants
        {
            public const string AuthorizationFilter = "Server Authorization Filter";
            public const string ActionFilter = "Server Action Filter";
            public const string ExceptionFilter = "Server Exception Filter";
            public const string ResultFilter = "Server RemoteResult Filter";
            public const string AlwaysRunResultFilter = "Server Always Run RemoteResult Filter";
        }

        private sealed class ServerInvokeExceptionContextSealed : ServerInvokeExceptionContext
        {
            public ServerInvokeExceptionContextSealed(ServiceEntryContext serviceEntryContext, IList<IFilterMetadata> filters)
                : base(serviceEntryContext, filters)
            {
            }
        }

        private sealed class ServerResultExecutedContextSealed : ServerResultExecutedContext
        {
            public ServerResultExecutedContextSealed(ServiceEntryContext serviceEntryContext, IList<IFilterMetadata> filters, object result)
                : base(serviceEntryContext, filters, result)
            {
            }
        }

        private sealed class ServerAuthorizationFilterContextSealed : ServerAuthorizationFilterContext
        {
            public ServerAuthorizationFilterContextSealed(ServiceEntryContext context, IList<IFilterMetadata> filters)
                : base(context, filters)
            {
            }
        }

        private sealed class ServerResultExecutingContextSealed : ServerResultExecutingContext
        {
            public ServerResultExecutingContextSealed(ServiceEntryContext serviceEntryContext, IList<IFilterMetadata> filters, object result)
                : base(serviceEntryContext, filters, result)
            {
            }
        }
    }