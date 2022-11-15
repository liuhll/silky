using System;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public abstract class FullAuditedEntityWithGuid : AuditedEntityWithGuid, ISoftDeletedObjectWithGuid
{
    public bool IsDeleted { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
    
    protected FullAuditedEntityWithGuid()
    {
    }
}