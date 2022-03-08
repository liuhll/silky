using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Auditing.Filters;

public class AuditingFilter : IServerFilter
{
    public int Order { get; } = 9999;

    private readonly AuditingOptions _auditingOptions;
    private readonly IAuditSerializer _auditSerializer;

    private AuditLogActionInfo _auditLogActionInfo;
    private Stopwatch _stopwatch;

    public AuditingFilter(
        IOptions<AuditingOptions> auditingOptions,
        IAuditSerializer auditSerializer)
    {
        _auditSerializer = auditSerializer;
        _auditingOptions = auditingOptions.Value;
    }

    public void OnActionExecuting(ServerExecutingContext context)
    {
        if (!context.ServiceEntry.IsEnableAuditing(_auditingOptions.IsEnabled))
        {
            return;
        }
        _stopwatch = Stopwatch.StartNew();
        _auditLogActionInfo = new AuditLogActionInfo()
        {
            Parameters = _auditSerializer.Serialize(context.Parameters),
            ExecutionTime = DateTimeOffset.Now,
            MethodName = context.ServiceEntry.MethodInfo.Name,
            HostName = EngineContext.Current.HostName,
            HostAddress = RpcContext.Context.Connection.LocalAddress,
            ServiceName = context.ServiceEntry.ServiceEntryDescriptor.ServiceName,
            ServiceEntryId = context.ServiceEntry.Id,
            ServiceKey = context.ServiceKey,
            IsDistributedTransaction = context.ServiceEntry.IsTransactionServiceEntry(),
        };
    }

    public void OnActionExecuted(ServerExecutedContext context)
    {
        if (!context.ServiceEntry.IsEnableAuditing(_auditingOptions.IsEnabled))
        {
            return;
        }
        _stopwatch.Stop();
        _auditLogActionInfo.ExecutionDuration = (int)_stopwatch.ElapsedMilliseconds;
         
        RpcContext.Context.SetAuditingActionLog(_auditLogActionInfo);
    }

    public void OnActionException(ServerExceptionContext context)
    {
        if (!context.ServiceEntry.IsEnableAuditing(_auditingOptions.IsEnabled))
        {
            return;
        }

        _stopwatch.Stop();
        _auditLogActionInfo.ExecutionDuration = (int)_stopwatch.ElapsedMilliseconds;
        _auditLogActionInfo.ExceptionMessage = context.Exception.Message;
        RpcContext.Context.SetAuditingActionLog(_auditLogActionInfo);
    }
}