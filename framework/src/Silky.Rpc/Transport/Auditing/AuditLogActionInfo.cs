using System;

namespace Silky.Rpc.Transport.Auditing;

public class AuditLogActionInfo
{
    public string ServiceEntryId { get; set; }

    public string ServiceName { get; set; }

    public string ServiceKey { get; set; }

    public string MethodName { get; set; }

    public string Parameters { get; set; }

    public bool IsDistributedTransaction { get; set; }
    public DateTimeOffset ExecutionTime { get; set; }

    public int ExecutionDuration { get; set; }
}