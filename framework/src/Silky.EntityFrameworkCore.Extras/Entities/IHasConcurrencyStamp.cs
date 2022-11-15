namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IHasConcurrencyStamp
{
    string ConcurrencyStamp { get; set; }
}