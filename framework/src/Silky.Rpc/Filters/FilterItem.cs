using System;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public class FilterItem
{
    public FilterItem(FilterDescriptor descriptor)
    {
        if (descriptor == null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        Descriptor = descriptor;
    }
    
    public FilterItem(FilterDescriptor descriptor, IServerFilterMetadata filter)
        : this(descriptor)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        Filter = filter;
    }
    
    public FilterDescriptor Descriptor { get; } = default!;
    
    public IFilterMetadata Filter { get; set; } = default!;
    
    public bool IsReusable { get; set; }
    
}