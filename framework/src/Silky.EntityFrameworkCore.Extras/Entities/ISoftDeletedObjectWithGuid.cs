using System;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface ISoftDeletedObjectWithGuid
{
    bool IsDeleted { get; set; }

    Guid? DeletedBy { get; set; }

    DateTimeOffset? DeletedTime { get; set; }
}