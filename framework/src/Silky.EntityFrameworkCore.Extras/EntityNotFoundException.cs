using System;
using Silky.Core.Exceptions;

namespace Silky.EntityFrameworkCore.Extras;

public class EntityNotFoundException : SilkyException
{
    public object Id { get; set; }

    public Type EntityType { get; set; }
    
    public EntityNotFoundException(Type entityType)
        : this(entityType, null, null)
    {
    }

    public EntityNotFoundException(Type entityType, object id)
        : this(entityType, id, null)
    {
    }


    public EntityNotFoundException(Type entityType, object id, Exception innerException)
        : base(
            id == null
                ? $"No record found for given id. Entity type: {entityType.FullName}"
                : $"No record found for given id. Entity type: {entityType.FullName}, id: {id}",
            innerException, StatusCode.BusinessError)
    {
        EntityType = entityType;
        Id = id;
    }
}