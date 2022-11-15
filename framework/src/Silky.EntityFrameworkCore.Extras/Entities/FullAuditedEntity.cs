using System;
using Silky.EntityFrameworkCore.Extras.Entities;

namespace Silky.Hero.Common.EntityFrameworkCore.Entities;

public abstract class FullAuditedEntity : AuditedEntity, ISoftDeletedObject
{
    public bool IsDeleted { get; set; }

    public long? DeletedBy { get; set; }

    public DateTimeOffset? DeletedTime { get; set; }

    protected FullAuditedEntity()
    {
    }
}