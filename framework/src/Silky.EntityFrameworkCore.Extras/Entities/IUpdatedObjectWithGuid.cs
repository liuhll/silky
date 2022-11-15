using System;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IUpdatedObjectWithGuid : IHasUpdatedTime
{
    Guid? UpdatedBy { get; set; }
}