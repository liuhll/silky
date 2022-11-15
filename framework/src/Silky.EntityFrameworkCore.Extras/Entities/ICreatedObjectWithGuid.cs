using System;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface ICreatedObjectWithGuid : IHasCreatedTime
{
    public Guid? CreatedBy { get; set; }
}