using System;
using Silky.EntityFrameworkCore.Entities;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public abstract class AuditedEntityWithGuid : Entity<Guid>, IAuditedObjectWithGuid, IEntity, IHasTenantObjectWithGuid
{
    protected AuditedEntityWithGuid()
    {
    }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? TenantId { get; set; }
}