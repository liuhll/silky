using Silky.Rpc.Auditing;

namespace Silky.Http.Auditing;

public interface IAuditingStore
{
    Task SaveAsync(AuditLogInfo auditLogInfo);
}