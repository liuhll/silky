using System;

namespace Silky.EntityFrameworkCore.Extras.Entities;

public interface IHasUpdatedTime 
{
    DateTimeOffset? UpdatedTime { get; set; }
}