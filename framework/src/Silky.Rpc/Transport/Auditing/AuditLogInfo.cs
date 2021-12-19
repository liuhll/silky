using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silky.Rpc.Transport.Auditing;

public class AuditLogInfo
{
    public long? UserId { get; set; }

    public string UserName { get; set; }

    public long? TenantId { get; set; }

    public DateTimeOffset ExecutionTime { get; set; }

    public int ExecutionDuration { get; set; }

    public string BrowserInfo { get; set; }

    public string HttpMethod { get; set; }

    public string ClientIpAddress { get; set; }

    public string ClientName { get; set; }

    public string ClientId { get; set; }

    public string CorrelationId { get; set; }

    public int? HttpStatusCode { get; set; }

    public string Url { get; set; }

    public List<AuditLogActionInfo> Actions { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            $"AUDIT LOG: [{HttpStatusCode?.ToString() ?? "---"}: {(HttpMethod ?? "-------").PadRight(7)}] {Url}");
        sb.AppendLine($"- UserName - UserId                 : {UserName} - {UserId}");
        sb.AppendLine($"- ClientIpAddress        : {ClientIpAddress}");
        sb.AppendLine($"- ExecutionDuration      : {ExecutionDuration}");

        if (Actions.Any())
        {
            sb.AppendLine("- Actions:");
            foreach (var action in Actions)
            {
                sb.AppendLine($"  - {action.ServiceName}.{action.MethodName} ({action.ExecutionDuration} ms.)");
                sb.AppendLine($"    {action.Parameters}");
            }
        }

        return sb.ToString();
    }
}