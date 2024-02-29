using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silky.Core.Extensions;

namespace Silky.Rpc.Auditing;

public class AuditLogInfo
{
    public string? UserId { get; set; }

    public string UserName { get; set; }

    public object? TenantId { get; set; }

    public DateTimeOffset ExecutionTime { get; set; }

    public int ExecutionDuration { get; set; }

    public string BrowserInfo { get; set; }

    public string Url { get; set; }

    public string HttpMethod { get; set; }

    public string ClientIpAddress { get; set; }
    
    public string ClientId { get; set; }

    public string CorrelationId { get; set; }

    public int? HttpStatusCode { get; set; }
    
    public string RequestParameters { get; set; }

    public string ExceptionMessage { get; set; }

    public List<AuditLogActionInfo> Actions { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            $"AUDIT LOG: [{HttpStatusCode?.ToString() ?? "---"}: {(HttpMethod ?? "-------").PadRight(7)}] {Url}");
        sb.AppendLine($"- UserName - UserId                 : {UserName} - {UserId}");
        sb.AppendLine($"- ClientIpAddress        : {ClientIpAddress}");
        sb.AppendLine($"- ExecutionDuration      : {ExecutionDuration}");
        if (!ExceptionMessage.IsNullOrEmpty())
        {
            sb.AppendLine($"- ExceptionMessage      : {ExceptionMessage}");
        }
        
        if (Actions.Any())
        {
            sb.AppendLine("- Actions:");
            foreach (var action in Actions)
            {
                sb.AppendLine(
                    $"  - {action.HostName}.{action.ServiceName}.{action.MethodName} ({action.ExecutionDuration} ms.)");
                sb.AppendLine($"    {action.Parameters}");
                sb.AppendLine($"    {action.HostAddress}");
                if (!action.ExceptionMessage.IsNullOrEmpty())
                {
                    sb.AppendLine($"    {action.ExceptionMessage}");
                }
            }
        }

        return sb.ToString();
    }
}