using System;
using System.Collections.Generic;

namespace Silky.Rpc.Filters;

internal class FilterDescriptorOrderComparer : IComparer<FilterDescriptor>
{
    public static FilterDescriptorOrderComparer Comparer { get; } = new FilterDescriptorOrderComparer();

    public int Compare(FilterDescriptor x, FilterDescriptor y)
    {
        if (x == null)
        {
            throw new ArgumentNullException(nameof(x));
        }

        if (y == null)
        {
            throw new ArgumentNullException(nameof(y));
        }

        if (x.Order == y.Order)
        {
            return x.Scope.CompareTo(y.Scope);
        }
        else
        {
            return x.Order.CompareTo(y.Order);
        }
    }
}