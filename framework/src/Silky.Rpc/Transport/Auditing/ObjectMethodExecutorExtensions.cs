using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;
using Silky.Core.Rpc;
using Silky.Core.Serialization;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Transport.Auditing;

public static class ObjectMethodExecutorExtensions
{
    public static async Task<object> ExecuteMethodWithAuditingAsync([NotNull] this ObjectMethodExecutor executor,
        [NotNull] object target,
        object?[]? parameters,
        ServiceEntry serviceEntry)
    {
        var auditingOptions = EngineContext.Current.GetOptions<AuditingOptions>();
        var serializer = EngineContext.Current.Resolve<ISerializer>();
        AuditLogActionInfo auditLogActionInfo = null;
        try
        {
            if (auditingOptions.IsEnabled)
            {
                auditLogActionInfo = new AuditLogActionInfo()
                {
                    Parameters = serializer.Serialize(parameters),
                    ExecutionTime = DateTimeOffset.Now,
                    MethodName = executor.MethodInfo.Name,
                    ServiceName = serviceEntry.ServiceEntryDescriptor.ServiceName,
                    ServiceEntryId = serviceEntry.Id,
                    ServiceKey = RpcContext.Context.GetServiceKey(),
                    IsDistributedTransaction = serviceEntry.IsTransactionServiceEntry()
                };
            }

            return await executor?.ExecuteMethodWithDbContextAsync(target, parameters);
        }
        finally
        {
            if (auditLogActionInfo != null)
            {
                auditLogActionInfo.ExecutionDuration =
                    (int)(DateTimeOffset.Now - auditLogActionInfo.ExecutionTime).TotalMilliseconds;
            }

            var auditLogsValue = RpcContext.Context.GetResultAttachment(AttachmentKeys.AuditActionLog);
            IList<string> auditLogs;
            auditLogs = auditLogsValue != null ? auditLogsValue.ConventTo<IList<string>>() : new List<string>();

            auditLogs.Add(serializer.Serialize(auditLogActionInfo));
            RpcContext.Context.SetResultAttachment(AttachmentKeys.AuditActionLog, auditLogs);
        }
    }
}