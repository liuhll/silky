using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;
using IFilterMetadata = Silky.Core.FilterMetadata.IFilterMetadata;

namespace Silky.Rpc.Runtime.Server;

internal class LocalInvoker : LocalInvokerBase
{
    private Dictionary<string, object> _arguments;
    private ServerInvokeExecutingContextSealed _serviceEntryInvokeExecutingContext;
    private ServerInvokeExecutedContextSealed _serviceEntryInvokeExecutedContext;

    public LocalInvoker(ILogger logger, ServiceEntryContext serviceEntryContext,
        IServiceEntryContextAccessor serviceEntryContextAccessor, IServerFilterMetadata[] filters)
        : base(logger, serviceEntryContext, serviceEntryContextAccessor, filters)
    {
    }

    protected override ValueTask ReleaseResources()
    {
        // 无需额外资源释放，直接返回默认ValueTask
        return default;
    }

    protected override Task InvokeInnerFilterAsync()
    {
        try
        {
            var next = State.ActionBegin;
            var scope = Scope.Invoker;
            object? state = null;
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
            // 对非任务包装的异常进行包装
            return Task.FromException(ex);
        }

        static async Task Awaited(LocalInvoker invoker, Task lastTask, State next, Scope scope, object? state, bool isCompleted)
        {
            await lastTask.ConfigureAwait(false);
            while (!isCompleted)
            {
                await invoker.Next(ref next, ref scope, ref state, ref isCompleted).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// 状态机实现过滤器调用逻辑
    /// </summary>
    private Task Next(ref State next, ref Scope scope, ref object? state, ref bool isCompleted)
    {
        switch (next)
        {
            case State.ActionBegin:
            {
                // 初始化参数绑定
                _arguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                var task = BindArgumentsAsync();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.ActionNext;
                    return task;
                }

                _cursor.Reset();
                goto case State.ActionNext;
            }
            case State.ActionNext:
            {
                // 获取下一个过滤器
                var current = _cursor.GetNextFilter<IServerFilter, IAsyncServerFilter>();
                if (current.FilterAsync != null)
                {
                    if (_serviceEntryInvokeExecutingContext == null)
                    {
                        _serviceEntryInvokeExecutingContext =
                            new ServerInvokeExecutingContextSealed(_serviceEntryContext, _filters, _arguments);
                    }
                    state = current.FilterAsync;
                    goto case State.ActionAsyncBegin;
                }
                else if (current.Filter != null)
                {
                    if (_serviceEntryInvokeExecutingContext == null)
                    {
                        _serviceEntryInvokeExecutingContext =
                            new ServerInvokeExecutingContextSealed(_serviceEntryContext, _filters, _arguments);
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
                Debug.Assert(_serviceEntryInvokeExecutingContext != null);

                var filter = (IAsyncServerFilter)state;
                var asyncTask = filter.OnActionExecutionAsync(_serviceEntryInvokeExecutingContext, InvokeNextServerFilterAwaitedAsync);
                if (!asyncTask.IsCompletedSuccessfully)
                {
                    next = State.ActionAsyncEnd;
                    return asyncTask;
                }
                goto case State.ActionAsyncEnd;
            }
            case State.ActionAsyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_serviceEntryInvokeExecutingContext != null);
                var filter = (IAsyncServerFilter)state;
                // 如果异步过滤器未调用 next，则认为短路执行
                if (_serviceEntryInvokeExecutedContext == null)
                {
                    _serviceEntryInvokeExecutedContext = new ServerInvokeExecutedContextSealed(
                        _serviceEntryContext,
                        _filters)
                    {
                        Canceled = true,
                        Result = _serviceEntryInvokeExecutingContext.Result,
                    };
                }
                goto case State.ActionEnd;
            }
            case State.ActionSyncBegin:
            {
                Debug.Assert(state != null);
                Debug.Assert(_serviceEntryInvokeExecutingContext != null);
                var filter = (IServerFilter)state;
                filter.OnActionExecuting(_serviceEntryInvokeExecutingContext);
                if (_serviceEntryInvokeExecutingContext.Result != null)
                {
                    _serviceEntryInvokeExecutedContext = new ServerInvokeExecutedContextSealed(
                        _serviceEntryContext,
                        _filters)
                    {
                        Canceled = true,
                        Result = _serviceEntryInvokeExecutingContext.Result,
                    };
                    goto case State.ActionEnd;
                }
                var task = InvokeNextServerFilterAsync();
                if (!task.IsCompletedSuccessfully)
                {
                    next = State.ActionSyncEnd;
                    return task;
                }
                goto case State.ActionSyncEnd;
            }
            case State.ActionSyncEnd:
            {
                Debug.Assert(state != null);
                Debug.Assert(_serviceEntryInvokeExecutingContext != null);
                Debug.Assert(_serviceEntryInvokeExecutedContext != null);
                var filter = (IServerFilter)state;
                filter.OnActionExecuted(_serviceEntryInvokeExecutedContext);
                goto case State.ActionEnd;
            }
            case State.ActionInside:
            {
                var task = InvokeServiceEntryMethodAsync();
                if (!task.IsCompletedSuccessfully)
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
                    if (_serviceEntryInvokeExecutedContext == null)
                    {
                        _serviceEntryInvokeExecutedContext = new ServerInvokeExecutedContextSealed(_serviceEntryContext, _filters)
                        {
                            Result = _result,
                        };
                    }
                    isCompleted = true;
                    return Task.CompletedTask;
                }
                Rethrow(_serviceEntryInvokeExecutedContext);
                if (_serviceEntryInvokeExecutedContext != null)
                {
                    _result = _serviceEntryInvokeExecutedContext.Result;
                }
                isCompleted = true;
                return Task.CompletedTask;
            }
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// 异步调用下一个服务器过滤器，返回执行上下文
    /// </summary>
    private Task<ServerInvokeExecutedContext> InvokeNextServerFilterAwaitedAsync()
    {
        Debug.Assert(_serviceEntryInvokeExecutingContext != null);
        if (_serviceEntryInvokeExecutingContext.Result != null)
        {
            // 不允许异步过滤器既设置结果又调用 next
            return Throw();
        }

        var task = InvokeNextServerFilterAsync();
        if (!task.IsCompletedSuccessfully)
        {
            return Awaited(this, task);
        }

        Debug.Assert(_serviceEntryInvokeExecutedContext != null);
        return Task.FromResult<ServerInvokeExecutedContext>(_serviceEntryInvokeExecutedContext);

        static async Task<ServerInvokeExecutedContext> Awaited(LocalInvoker invoker, Task task)
        {
            await task.ConfigureAwait(false);
            Debug.Assert(invoker._serviceEntryInvokeExecutedContext != null);
            return invoker._serviceEntryInvokeExecutedContext;
        }

#pragma warning disable CS1998
        static async Task<ServerInvokeExecutedContext> Throw()
        {
            throw new InvalidOperationException("InvokeAsync Server Filter Fail");
        }
#pragma warning restore CS1998
    }

    /// <summary>
    /// 调用下一个服务器过滤器
    /// </summary>
    private Task InvokeNextServerFilterAsync()
    {
        try
        {
            var next = State.ActionNext;
            object? state = null;
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
            _serviceEntryInvokeExecutedContext = new ServerInvokeExecutedContextSealed(_serviceEntryContext, _filters)
            {
                ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
            };
        }

        Debug.Assert(_serviceEntryInvokeExecutedContext != null);
        return Task.CompletedTask;

        static async Task Awaited(LocalInvoker invoker, Task lastTask, State next, Scope scope, object? state, bool isCompleted)
        {
            try
            {
                await lastTask.ConfigureAwait(false);
                while (!isCompleted)
                {
                    await invoker.Next(ref next, ref scope, ref state, ref isCompleted).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                invoker._serviceEntryInvokeExecutedContext =
                    new ServerInvokeExecutedContextSealed(invoker._serviceEntryContext, invoker._filters)
                    {
                        ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                    };
            }
            Debug.Assert(invoker._serviceEntryInvokeExecutedContext != null);
        }
    }

    /// <summary>
    /// 参数绑定（同步完成）
    /// </summary>
    private Task BindArgumentsAsync()
    {
        var serviceEntry = _serviceEntryContext.ServiceEntry;
        if (serviceEntry.Parameters.Count == 0)
        {
            return Task.CompletedTask;
        }

        for (var index = 0; index < serviceEntry.Parameters.Count; index++)
        {
            var parameterDescriptor = serviceEntry.Parameters[index];
            _arguments[parameterDescriptor.Name] = _serviceEntryContext.Parameters[index];
        }

        return Task.CompletedTask;
    }

    private sealed class ServerInvokeExecutingContextSealed : ServerInvokeExecutingContext
    {
        public ServerInvokeExecutingContextSealed(
            ServiceEntryContext serviceEntryContext,
            IList<IFilterMetadata> filters,
            IDictionary<string, object> arguments)
            : base(serviceEntryContext, filters, arguments)
        {
        }
    }

    private sealed class ServerInvokeExecutedContextSealed : ServerInvokeExecutedContext
    {
        public ServerInvokeExecutedContextSealed(ServiceEntryContext serviceEntryContext, IList<IFilterMetadata> filters)
            : base(serviceEntryContext, filters)
        {
        }
    }

    /// <summary>
    /// 真正调用服务入口方法，支持异步和同步执行
    /// </summary>
    private async Task InvokeServiceEntryMethodAsync()
    {
        var serviceEntry = _serviceEntryInvokeExecutingContext.ServiceEntry;
        var instance = _serviceEntryInvokeExecutingContext.ServiceInstance;
        var parameters = _serviceEntryInvokeExecutingContext.Parameters;

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

    private static void Rethrow(ServerInvokeExecutedContextSealed context)
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
