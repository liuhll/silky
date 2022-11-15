using Silky.Core.DependencyInjection;

namespace Silky.Core.Runtime.Session;

public class DefaultCurrentTenantId : ICurrentTenantId, IScopedDependency
{
    private object? _tenantId;
    private bool isSetTenantId = false;

    public object? TenantId
    {
        get
        {
            if (isSetTenantId)
            {
                return _tenantId;
            }

            if (NullSession.Instance.TenantId != null)
            {
                return NullSession.Instance.TenantId;
            }

            return null;
        }
    }

    public void SetTenantId(object? tenantId)
    {
        isSetTenantId = true;
        _tenantId = tenantId;
    }
}