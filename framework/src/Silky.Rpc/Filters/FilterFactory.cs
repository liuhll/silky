using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters;

internal static class FilterFactory
{
    public static FilterFactoryResult GetAllServerFilters(
        IServerFilterProvider filterProvider,
        ServiceEntryContext context)
    {
        if (filterProvider == null)
        {
            throw new ArgumentNullException(nameof(filterProvider));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        
        var staticFilterItems = new FilterItem[context.ServiceEntry.ServerFilters.Count];

        var orderedFilters = context.ServiceEntry.ServerFilters
            .OrderBy(
                filter => filter,
                FilterDescriptorOrderComparer.Comparer)
            .ToList();
        
        for (var i = 0; i < orderedFilters.Count; i++)
        {
            staticFilterItems[i] = new FilterItem(orderedFilters[i]);
        }

        var allFilterItems = new List<FilterItem>(staticFilterItems);
        
        var filters = CreateUncachedFiltersCore(filterProvider, context, allFilterItems);
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

    private static IServerFilterMetadata[] CreateUncachedFiltersCore(
        IServerFilterProvider filterProvider,
        ServiceEntryContext context,
        List<FilterItem> filterItems)
    {
        filterProvider.ProviderFilters(filterItems);
        
        var count = 0;
        for (var i = 0; i < filterItems.Count; i++)
        {
            if (filterItems[i].Filter != null && filterItems[i].Filter is IServerFilterMetadata)
            {
                count++;
            }
        }
        if (count == 0)
        {
            return Array.Empty<IServerFilterMetadata>();
        }
        else
        {
            var filters = new IServerFilterMetadata[count];
            var filterIndex = 0;
            for (int i = 0; i < filterItems.Count; i++)
            {
                var filter = filterItems[i].Filter;
                if (filter != null)
                {
                    filters[filterIndex++] = (IServerFilterMetadata)filter;
                }
            }
            return filters;
        }

        // var serverFilters = new List<IServerFilterMetadata>();
        // foreach (var filterItem in filterItems)
        // {
        //     if (filterItem.Filter != null)
        //     {
        //         serverFilters.Add(filterItem.Filter);
        //     }
        //     else
        //     {
        //         var filter = filterItem.Descriptor.Filter;
        //         if (!(filter is IServerFilterFactory filterFactory))
        //         {
        //             filterItem.Filter = (IServerFilterMetadata)filter;
        //             filterItem.IsReusable = true;
        //         }
        //         else
        //         {
        //             filterItem.Filter = filterFactory.CreateInstance(services);
        //             filterItem.IsReusable = filterFactory.IsReusable;
        //         }
        //
        //     }
        // }
    }
}