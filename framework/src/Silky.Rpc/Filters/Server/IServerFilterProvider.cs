using System.Collections.Generic;

namespace Silky.Rpc.Filters;

public interface IServerFilterProvider
{
    void ProviderFilters(List<FilterItem> filterItems);
}