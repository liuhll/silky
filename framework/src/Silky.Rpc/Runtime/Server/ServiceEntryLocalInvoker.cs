using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Silky.Rpc.Filters;
using IFilterMetadata = Silky.Rpc.Filters.IFilterMetadata;

namespace Silky.Rpc.Runtime.Server;

internal class ServiceEntryLocalInvoker : LocalInvoker
{
    private Dictionary<string, object> _arguments;
    private ServerExecutingContextSealed _serviceEntryExecutingContext;
    private ServerExecutedContextSealed _serviceEntryExecutedContext;

    public ServiceEntryLocalInvoker(ILogger logger, ServiceEntryContext serviceEntryContext,
        IServiceEntryContextAccessor serviceEntryContextAccessor, IServerFilterMetadata[] filters)
        : base(logger,
            serviceEntryContext,
            serviceEntryContextAccessor,
            filters)
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

        static async Task Awaited(ServiceEntryLocalInvoker invoker, Task lastTask, State next, Scope scope,
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
                var serviceEntryContext = _serviceEntryContext;
                _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                var task = BindArgumentsAsync();
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    next = State.ActionNext;
                    return task;
                }

                _cursor.Reset();
                goto case State.ActionNext;
            }
            case State.ActionNext:
            {
                var current = _cursor.GetNextFilter<IServerFilter, IAsyncServerFilter>();
                if (current.FilterAsync != null)
                {
                    if (_serviceEntryExecutingContext == null)
                    {
                        _serviceEntryExecutingContext =
                            new ServerExecutingContextSealed(_serviceEntryContext, _filters, _arguments);
                    }

                    state = current.FilterAsync;
                    goto case State.ActionAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    if (_serviceEntryExecutingContext == null)
                    {
                        _serviceEntryExecutingContext =
                            new ServerExecutingContextSealed(_serviceEntryContext, _filters, _arguments);
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
                Debug.Assert(_serviceEntryExecutingContext != null);

                var filter = (IAsyncServerFilter)state;
                var serviceEntryExecutingContext = _serviceEntryExecutingContext;
                var task = filter.OnActionExecutionAsync(serviceEntryExecutingContext,
                    InvokeNextServerFilterAwaitedAsync);
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
                Debug.Assert(_serviceEntryExecutingContext != null);
                var filter = (IAsyncServerFilter)state;
                if (_serviceEntryExecutedContext == null)
                {
                    // If we get here then the filter didn't call 'next' indicating a short circuit.

                    _serviceEntryExecutedContext = new ServerExecutedContextSealed(
                        _serviceEntryContext,
                        _filters)
                    {
                        Canceled = true,
                        Result = _serviceEntryExecutingContext.Result,
                    };
                }

                goto case State.ActionEnd;
            }
            case State.ActionSyncBegin:
            {
                Debug.Assert(state != null);
                Debug.Assert(_serviceEntryExecutingContext != null);
                var filter = (IServerFilter)state;
                var serviceEntryExecutingContext = _serviceEntryExecutingContext;
                filter.OnActionExecuting(serviceEntryExecutingContext);
                if (serviceEntryExecutingContext.Result != null)
                {
                    _serviceEntryExecutedContext = new ServerExecutedContextSealed(
                        _serviceEntryContext,
                        _filters)
                    {
                        Canceled = true,
                        Result = _serviceEntryExecutingContext.Result,
                    };
                    goto case State.ActionEnd;
                }

                var task = InvokeNextServerFilterAsync();
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
                Debug.Assert(_serviceEntryExecutingContext != null);
                Debug.Assert(_serviceEntryExecutedContext != null);

                var filter = (IServerFilter)state;
                var serviceEntryExecutedContext = _serviceEntryExecutedContext;
                filter.OnActionExecuted(serviceEntryExecutedContext);
                goto case State.ActionEnd;
            }
            case State.ActionInside:
            {
                var task = InvokeServiceEntryMethodAsync();
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
                    if (_serviceEntryExecutedContext == null)
                    {
                        _serviceEntryExecutedContext = new ServerExecutedContextSealed(_serviceEntryContext, _filters)
                        {
                            Result = _result,
                        };
                    }

                    isCompleted = true;
                    return Task.CompletedTask;
                }

                var serviceEntryExecutedContext = _serviceEntryExecutedContext;
                Rethrow(serviceEntryExecutedContext);
                if (serviceEntryExecutedContext != null)
                {
                    _result = serviceEntryExecutedContext.Result;
                }

                isCompleted = true;
                return Task.CompletedTask;
            }
            default:
                throw new InvalidOperationException();
        }
    }

    private Task<ServerExecutedContext> InvokeNextServerFilterAwaitedAsync()
    {
        Debug.Assert(_serviceEntryExecutingContext != null);
        if (_serviceEntryExecutingContext.Result != null)
        {
            // If we get here, it means that an async filter set a result AND called next(). This is forbidden.
            return Throw();
        }

        var task = InvokeNextServerFilterAsync();

        if (!task.IsCompletedSuccessfully)
        {
            return Awaited(this, task);
        }

        Debug.Assert(_serviceEntryExecutedContext != null);
        return Task.FromResult<ServerExecutedContext>(_serviceEntryExecutedContext);

        static async Task<ServerExecutedContext> Awaited(ServiceEntryLocalInvoker invoker, Task task)
        {
            await task;

            Debug.Assert(invoker._serviceEntryContext != null);
            return invoker._serviceEntryExecutedContext;
        }

#pragma warning disable CS1998
        static async Task<ServerExecutedContext> Throw()
        {
            var message = "Invoke Server Filter Fail";

            throw new InvalidOperationException(message);
        }
#pragma warning restore CS1998
    }

    private Task InvokeNextServerFilterAsync()
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
            _serviceEntryExecutedContext = new ServerExecutedContextSealed(_serviceEntryExecutedContext, _filters)
            {
                ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
            };
        }

        Debug.Assert(_serviceEntryExecutedContext != null);
        return Task.CompletedTask;

        static async Task Awaited(ServiceEntryLocalInvoker invoker, Task lastTask, State next, Scope scope,
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
                invoker._serviceEntryExecutedContext =
                    new ServerExecutedContextSealed(invoker._serviceEntryExecutedContext, invoker._filters)
                    {
                        ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                    };
            }

            Debug.Assert(invoker._serviceEntryExecutedContext != null);
        }
    }

    private Task BindArgumentsAsync()
    {
        var serviceEntry = _serviceEntryContext.ServiceEntry;
        if (serviceEntry.ParameterDescriptors.Count == 0)
        {
            return Task.CompletedTask;
        }

        for (var index = 0; index < serviceEntry.ParameterDescriptors.Count; index++)
        {
            var parameterDescriptor = serviceEntry.ParameterDescriptors[index];
            _arguments[parameterDescriptor.Name] = _serviceEntryContext.Parameters[index];
        }

        return Task.CompletedTask;
    }

    private sealed class ServerExecutingContextSealed : ServerExecutingContext
    {
        public ServerExecutingContextSealed(
            ServiceEntryContext serviceEntryContext,
            IList<IFilterMetadata> filters,
            IDictionary<string, object> arguments
        ) : base(serviceEntryContext, filters, arguments)
        {
        }
    }

    private sealed class ServerExecutedContextSealed : ServerExecutedContext
    {
        public ServerExecutedContextSealed(ServiceEntryContext serviceEntryContext, IList<IFilterMetadata> filters) :
            base(serviceEntryContext, filters)
        {
        }
    }

    private async Task InvokeServiceEntryMethodAsync()
    {
        var serviceEntry = _serviceEntryExecutingContext.ServiceEntry;
        var instance = _serviceEntryExecutingContext.ServiceInstance;
        var parameters = _serviceEntryExecutingContext.Parameters;

        if (serviceEntry.IsAsyncMethod)
        {
            var result = await serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray());
            _result = result;
        }
        else
        {
            var result = serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
            _result = result;
        }
    }


    private static void Rethrow(ServerExecutedContextSealed context)
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