namespace Silky.EntityFrameworkCore.Extras.Entities;
public interface ICreatedObject : IHasCreatedTime
{
    public long? CreatedBy { get; set; }

    
}