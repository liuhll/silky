namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IHasOrganization<TKey>
{
    public TKey OrganizationId { get; set; }
}