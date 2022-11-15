using System;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IHasCreatedTime
{
    public DateTimeOffset CreatedTime { get; set; }
}