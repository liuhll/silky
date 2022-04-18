using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Builder;

public class ServiceEntryDescriptorEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly object _lock;
    private readonly List<Action<EndpointBuilder>> _conventions;

    public ServiceEntryDescriptorEndpointConventionBuilder(object @lock, List<Action<EndpointBuilder>> conventions)
    {
        _lock = @lock;
        _conventions = conventions;
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        if (convention == null)
        {
            throw new ArgumentNullException(nameof(convention));
        }

        // The lock is shared with the data source. We want to lock here
        // to avoid mutating this list while its read in the data source.
        lock (_lock)
        {
            _conventions.Add(convention);
        }
    }
}