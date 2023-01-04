using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Rpc.Filters;

namespace Silky.Rpc.Runtime.Server;

internal class ServiceEntryLocalInvoker : LocalInvoker
{
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
                _cursor.Reset();
                _instance = serviceEntryContext.ServiceInstance;
                goto case State.ActionNext;
            }
            case State.ActionNext:
            {
                var current = _cursor.GetNextFilter<IServerFilter, IAsyncServerServerFilter>();
                if (current.FilterAsync != null)
                {
                    state = current.FilterAsync;
                }
                else if (current.Filter != null)
                {
                    state = current.Filter;
                }
                else
                {
                    goto case State.ActionInside;
                }
            }
            case State.ActionInside:
            {
                var task = InvokeActionMethodAsync();
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
                    isCompleted = true;
                    return Task.CompletedTask;
                }
            }
        }
    }

    private Task InvokeActionMethodAsync()
    {
        throw new NotImplementedException();
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