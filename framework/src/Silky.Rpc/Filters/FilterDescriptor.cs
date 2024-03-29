using System;
using System.Diagnostics;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

[DebuggerDisplay("Filter = {Filter.ToString(),nq}, Order = {Order}")]
public class FilterDescriptor
{
    public FilterDescriptor(IFilterMetadata filter, int filterScope)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        Filter = filter;
        Scope = filterScope;


        if (Filter is IOrderedFilter orderedFilter)
        {
            Order = orderedFilter.Order;
        }
        
    }
    
    public IFilterMetadata Filter { get; }
    
    public int Order { get; set; }

    public int Scope { get; }
}