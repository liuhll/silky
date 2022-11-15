using System;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IHasTenantObjectWithGuid 
{
    Guid? TenantId { get; set; }
}