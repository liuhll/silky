using System;
using System.Collections.Generic;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Core.Logging;

public class DefaultInitLoggerFactory : IInitLoggerFactory
{
    private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();

    public virtual IInitLogger<T> Create<T>()
    {
        return (IInitLogger<T>)_cache.GetOrAdd(typeof(T), () => new DefaultInitLogger<T>()); ;
    }
}