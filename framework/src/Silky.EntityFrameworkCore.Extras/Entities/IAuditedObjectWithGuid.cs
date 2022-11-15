namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IAuditedObjectWithGuid : ICreatedObjectWithGuid, IUpdatedObjectWithGuid, IHasTenantObjectWithGuid
{
    
}