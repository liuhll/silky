using Microsoft.Extensions.Options;
using Silky.Auditing.Configuration;
using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Abstraction;

namespace Silky.Auditing.Filters;

public class AuditingFilter : IServerFilter
{
    public int Order { get; } = 9999;

    private readonly AuditingOptions _auditingOptions;
    private readonly IAuditSerializer _auditSerializer;

    private AuditLogActionInfo _auditLogActionInfo;

    public AuditingFilter(
        IOptions<AuditingOptions> auditingOptions,
        IAuditSerializer auditSerializer)
    {
        _auditSerializer = auditSerializer;
        _auditingOptions = auditingOptions.Value;
    }

    public void OnActionExecuting(ServerExecutingContext context)
    {
        if (!_auditingOptions.IsEnabled)
        {
            return;
        }

        _auditLogActionInfo = new AuditLogActionInfo()
        {
            Parameters = _auditSerializer.Serialize(context.Parameters),
            ExecutionTime = DateTimeOffset.Now,
            MethodName = context.ServiceEntry.MethodInfo.Name,
            ServiceName = context.ServiceEntry.ServiceEntryDescriptor.ServiceName,
            ServiceEntryId = context.ServiceEntry.Id,
            ServiceKey = context.ServiceKey,
            IsDistributedTransaction = context.ServiceEntry.IsTransactionServiceEntry()
        };
    }

    public void OnActionExecuted(ServerExecutedContext context)
    {
        if (!_auditingOptions.IsEnabled)
        {
            return;
        }

        if (_auditLogActionInfo.IsDistributedTransaction)
        {
            var transactionContext = RpcContext.Context.GetTransactionContext();
            _auditLogActionInfo.TransId = transactionContext?.TransId;
        }

        _auditLogActionInfo.ExecutionDuration =
            (int)(DateTimeOffset.Now - _auditLogActionInfo.ExecutionTime).TotalMilliseconds;

        var auditLogsValue = RpcContext.Context.GetResultAttachment(AttachmentKeys.AuditActionLog);
        IList<string> auditLogs;
        auditLogs = auditLogsValue != null ? auditLogsValue.ConventTo<IList<string>>() : new List<string>();

        auditLogs.Add(_auditSerializer.Serialize(_auditLogActionInfo));
        RpcContext.Context.SetResultAttachment(AttachmentKeys.AuditActionLog, auditLogs);
    }
}