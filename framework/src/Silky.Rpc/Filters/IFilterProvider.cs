using System.Collections.Generic;

namespace Silky.Rpc.Filters;

public interface IFilterProvider
{
    void ProviderFilters(List<FilterItem> filterItems);
}