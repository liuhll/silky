using System;
using System.Collections.Generic;
using System.Linq;

namespace Silky.Rpc.Filters;

internal static class ClientFilterFactory
{
    public static FilterFactoryResult GetAllFilters(IFilterProvider filterProvider, FilterDescriptor[] filterDescriptors)
    {
        if (filterProvider == null)
        {
            throw new ArgumentNullException(nameof(filterProvider));
        }

        if (filterDescriptors == null)
        {
            throw new ArgumentNullException(nameof(filterDescriptors));
        }
        var staticFilterItems = new FilterItem[filterDescriptors.Length];
        var orderedFilters = filterDescriptors
            .OrderBy(
                filter => filter,
                FilterDescriptorOrderComparer.Comparer)
            .ToList();
        for (var i = 0; i < orderedFilters.Count; i++)
        {
            staticFilterItems[i] = new FilterItem(orderedFilters[i]);
        }
        var allFilterItems = new List<FilterItem>(staticFilterItems);
        
        var filters = CreateUncachedFiltersCore(filterProvider, allFilterItems);
        for (var i = 0; i < staticFilterItems.Length; i++)
        {
            var item = staticFilterItems[i];
            if (!item.IsReusable)
            {
                item.Filter = null;
            }
        }
        return new FilterFactoryResult(staticFilterItems,filters);
    }

    public static IClientFilterMetadata[] CreateUncachedFilters(IFilterProvider filterProvider, FilterItem[] cachedFilterItems)
    {
        if (filterProvider == null)
        {
            throw new ArgumentNullException(nameof(filterProvider));
        }
        
        if (cachedFilterItems == null)
        {
            throw new ArgumentNullException(nameof(cachedFilterItems));
        }
        var filterItems = new List<FilterItem>(cachedFilterItems.Length);
        
        // Deep copy the cached filter items as filter providers could modify them
        for (var i = 0; i < cachedFilterItems.Length; i++)
        {
            var filterItem = cachedFilterItems[i];
            filterItems.Add(
                new FilterItem(filterItem.Descriptor)
                {
                    Filter = filterItem.Filter,
                    IsReusable = filterItem.IsReusable
                });
        }
        return CreateUncachedFiltersCore(filterProvider, filterItems);
    }
    
    private static IClientFilterMetadata[] CreateUncachedFiltersCore(
        IFilterProvider filterProvider,
        List<FilterItem> filterItems)
    {
        filterProvider.ProviderFilters(filterItems);
        
        var count = 0;
        for (var i = 0; i < filterItems.Count; i++)
        {
            if (filterItems[i].Filter != null && filterItems[i].Filter is IClientFilterMetadata)
            {
                count++;
            }
        }
        if (count == 0)
        {
            return Array.Empty<IClientFilterMetadata>();
        }
        else
        {
            var filters = new IClientFilterMetadata[count];
            var filterIndex = 0;
            for (int i = 0; i < filterItems.Count; i++)
            {
                var filter = filterItems[i].Filter;
                if (filter != null)
                {
                    filters[filterIndex++] = (IClientFilterMetadata)filter;
                }
            }
            return filters;
        }
        
    }
}