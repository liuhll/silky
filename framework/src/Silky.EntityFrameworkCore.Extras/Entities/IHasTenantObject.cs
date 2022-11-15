namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IHasTenantObject
{
    long? TenantId { get; set; }
}