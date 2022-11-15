using Silky.Hero.Common.EntityFrameworkCore.Entities;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IAuditedObject : ICreatedObject, IUpdatedObject, IHasTenantObject
{
}