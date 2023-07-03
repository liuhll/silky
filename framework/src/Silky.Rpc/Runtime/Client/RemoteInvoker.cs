using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Client;

internal class RemoteInvoker : RemoteInvokerBase
{
    private ClientInvokeExecutingContextSealed _invokeExecutingContext;
    private ClientInvokeExecutedContextSealed _invokeExecutedContext;

    public RemoteInvoker(ILogger logger, ClientInvokeContext context,
        IClientInvokeContextAccessor clientInvokeContextAccessor,
        string messageId,
        ITransportClient client,
        IClientFilterMetadata[] filters, int timeoutMillSeconds)
        : base(logger,
            context,
            clientInvokeContextAccessor,
            messageId,
            client,
            filters,
            timeoutMillSeconds)
    {
    }

    protected override ValueTask ReleaseResources()
    {
        return default;
    }

    protected override Task InvokeInnerFilterAsync()
    {
        try
        {
            var next = State.ActionBegin;
            var scope = Scope.Invoker;
            var state = (object?)null;
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

        static async Task Awaited(RemoteInvoker invoker, Task lastTask, State next, Scope scope,
            object? state, bool isCompleted)
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
            case State.ActionBegin:
            {
                var clientInvokeContext = _clientInvokeContext;
                _cursor.Reset();
                goto case State.ActionNext;
            }
            case State.ActionNext:
            {
                var current = _cursor.GetNextFilter<IClientFilter, IAsyncClientFilter>();
                if (current.FilterAsync != null)
                {
                    if (_invokeExecutingContext == null)
                    {
                        _invokeExecutingContext =
                            new ClientInvokeExecutingContextSealed(_clientInvokeContext, _filters);
                    }

                    state = current.FilterAsync;
                    goto case State.ActionAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    if (_invokeExecutingContext == null)
                    {
                        _invokeExecutingContext =
                            new ClientInvokeExecutingContextSealed(_clientInvokeContext, _filters);
                    }

                    state = current.Filter;
                    goto case State.ActionSyncBegin;
                }
                else
                {
                    goto case State.ActionInside;
                }
            }
            case State.ActionAsyncBegin:
            {
                Debug.Assert(state != null);
                Debug.Assert(_invokeExecutingContext != null);

                var filter = (IAsyncClientFilter)state;
                var invokeExecutingContext = _invokeExecutingContext;
                var task = filter.OnActionExecutionAsync(invokeExecutingContext,
                    InvokeNextClientFilterAwaitedAsync);
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    next = State.ActionAsyncEnd;
                    return task;
                }

                goto case State.ActionAsyncEnd;
            }
            case State.ActionAsyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_invokeExecutingContext != null);
                var filter = (IAsyncClientFilter)state;
                if (_invokeExecutedContext == null)
                {
                    // If we get here then the filter didn't call 'next' indicating a short circuit.

                    _invokeExecutedContext = new ClientInvokeExecutedContextSealed(
                        _clientInvokeContext,
                        _filters)
                    {
                        Canceled = true,
                        Result = _invokeExecutingContext.Result,
                    };
                }

                goto case State.ActionEnd;
            }
            case State.ActionSyncBegin:
            {
                Debug.Assert(state != null);
                Debug.Assert(_invokeExecutingContext != null);
                var filter = (IClientFilter)state;
                var invokeExecutingContext = _invokeExecutingContext;
                filter.OnActionExecuting(invokeExecutingContext);
                if (invokeExecutingContext.Result != null)
                {
                    _invokeExecutedContext = new ClientInvokeExecutedContextSealed(
                        _clientInvokeContext,
                        _filters)
                    {
                        Canceled = true,
                        Result = _invokeExecutingContext.Result,
                    };
                    goto case State.ActionEnd;
                }

                var task = InvokeNextClientFilterAsync();
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    next = State.ActionSyncEnd;
                    return task;
                }

                goto case State.ActionSyncEnd;
            }
            case State.ActionSyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_invokeExecutingContext != null);
                Debug.Assert(_invokeExecutedContext != null);

                var filter = (IClientFilter)state;
                var invokeExecutedContext = _invokeExecutedContext;
                filter.OnActionExecuted(invokeExecutedContext);
                goto case State.ActionEnd;
            }
            case State.ActionInside:
            {
                var task = InvokeRemoteCallMethodAsync();
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    next = State.ActionEnd;
                    return task;
                }

                goto case State.ActionEnd;
            }
            case State.ActionEnd:
            {
                if (scope == Scope.Action)
                {
                    if (_invokeExecutedContext == null)
                    {
                        _invokeExecutedContext = new ClientInvokeExecutedContextSealed(_clientInvokeContext, _filters)
                        {
                            Result = _result,
                        };
                    }

                    isCompleted = true;
                    return Task.CompletedTask;
                }

                var invokeExecutedContext = _invokeExecutedContext;
                Rethrow(invokeExecutedContext);
                if (invokeExecutedContext != null)
                {
                    _result = invokeExecutedContext.Result;
                }

                isCompleted = true;
                return Task.CompletedTask;
            }
            default:
                throw new InvalidOperationException();
        }
    }

    private async Task InvokeRemoteCallMethodAsync()
    {
        var result =
            await _client.SendAsync(_clientInvokeContext.RemoteInvokeMessage, _messageId!, _timeoutMillSeconds);
        _result = result;
    }

    private Task<ClientInvokeExecutedContext> InvokeNextClientFilterAwaitedAsync()
    {
        Debug.Assert(_invokeExecutingContext != null);
        if (_invokeExecutingContext.Result != null)
        {
            // If we get here, it means that an async filter set a result AND called next(). This is forbidden.
            return Throw();
        }

        var task = InvokeNextClientFilterAsync();

        if (!task.IsCompletedSuccessfully)
        {
            return Awaited(this, task);
        }

        Debug.Assert(_invokeExecutedContext != null);
        return Task.FromResult<ClientInvokeExecutedContext>(_invokeExecutedContext);

        static async Task<ClientInvokeExecutedContext> Awaited(RemoteInvoker invoker, Task task)
        {
            await task;

            Debug.Assert(invoker._clientInvokeContext != null);
            return invoker._invokeExecutedContext;
        }

#pragma warning disable CS1998
        static async Task<ClientInvokeExecutedContext> Throw()
        {
            var message = "InvokeAsync Server Filter Fail";

            throw new InvalidOperationException(message);
        }
#pragma warning restore CS1998
    }

    private Task InvokeNextClientFilterAsync()
    {
        try
        {
            var next = State.ActionNext;
            var state = (object)null;
            var scope = Scope.Action;
            var isCompleted = false;
            while (!isCompleted)
            {
                var lastTask = Next(ref next, ref scope, ref state, ref isCompleted);
                if (!lastTask.IsCompletedSuccessfully)
                {
                    return Awaited(this, lastTask, next, scope, state, isCompleted);
                }
            }
        }
        catch (Exception exception)
        {
            _invokeExecutedContext = new ClientInvokeExecutedContextSealed(_clientInvokeContext, _filters)
            {
                ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
            };
        }

        Debug.Assert(_invokeExecutedContext != null);
        return Task.CompletedTask;

        static async Task Awaited(RemoteInvoker invoker, Task lastTask, State next, Scope scope,
            object state, bool isCompleted)
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
                invoker._invokeExecutedContext =
                    new ClientInvokeExecutedContextSealed(invoker._clientInvokeContext, invoker._filters)
                    {
                        ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                    };
            }

            Debug.Assert(invoker._invokeExecutedContext != null);
        }
    }

    private static void Rethrow(ClientInvokeExecutedContextSealed context)
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

    private sealed class ClientInvokeExecutingContextSealed : ClientInvokeExecutingContext
    {
        public ClientInvokeExecutingContextSealed(
            ClientInvokeContext context,
            IList<IFilterMetadata> filters
        ) : base(context, filters)
        {
        }
    }

    private sealed class ClientInvokeExecutedContextSealed : ClientInvokeExecutedContext
    {
        public ClientInvokeExecutedContextSealed(ClientInvokeContext serviceEntryContext,
            IList<IFilterMetadata> filters) :
            base(serviceEntryContext, filters)
        {
        }
    }

    private enum State
    {
        ActionBegin,
        ActionNext,
        ActionAsyncBegin,
        ActionAsyncEnd,
        ActionSyncBegin,
        ActionSyncEnd,
        ActionInside,
        ActionEnd,
    }

    private enum Scope
    {
        Invoker,
        Action,
    }
}