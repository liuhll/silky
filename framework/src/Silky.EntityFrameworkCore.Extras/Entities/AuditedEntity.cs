using Silky.EntityFrameworkCore.Entities;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public abstract class AuditedEntity : Entity<long>, IAuditedObject, IEntity, IHasTenantObject
{
    protected AuditedEntity()
    {
    }

    public long? CreatedBy { get; set; }

    public long? UpdatedBy { get; set; }

    public long? TenantId { get; set; }
}