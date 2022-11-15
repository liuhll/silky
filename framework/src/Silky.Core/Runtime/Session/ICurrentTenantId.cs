namespace Silky.Core.Runtime.Session;

public interface ICurrentTenantId
{
    object? TenantId { get; }

    void SetTenantId(object? tenantId);
}