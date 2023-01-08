using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Silky.Core;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Extensions;
using Silky.Rpc.Filters;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Auditing.Filters;

public class AuditingServerFilter : IAsyncServerFilter
{
    private readonly AuditingOptions _auditingOptions;
    private readonly IAuditSerializer _auditSerializer;


    public AuditingServerFilter(
        IOptions<AuditingOptions> auditingOptions,
        IAuditSerializer auditSerializer)
    {
        _auditSerializer = auditSerializer;
        _auditingOptions = auditingOptions.Value;
    }


    public async Task OnActionExecutionAsync(ServerInvokeExecutingContext context, ServerExecutionDelegate next)
    {
        if (!context.ServiceEntry.IsEnableAuditing(_auditingOptions.IsEnabled))
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var auditLogActionInfo = new AuditLogActionInfo()
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

        var result = await next();
        stopwatch.Stop();
        auditLogActionInfo.ExceptionMessage = result.Exception?.Message;
        auditLogActionInfo.ExecutionDuration = (int)stopwatch.ElapsedMilliseconds;
        RpcContext.Context.SetAuditingActionLog(auditLogActionInfo);
    }
}