using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters
{
    public class ServerFilterProvider : IServerFilterProvider, ISingletonDependency
    {
        public void ProviderFilters(List<FilterItem> filterItems)
        {
            foreach (var filterItem in filterItems)
            {
                if (filterItem.Filter != null)
                {
                    continue;
                }
                var filter = filterItem.Descriptor.Filter;
                if (!(filter is IServerFilterFactory filterFactory))
                {
                    filterItem.Filter = filter;
                    filterItem.IsReusable = true;
                }
                else
                {
                    filterItem.Filter = filterFactory.CreateInstance(EngineContext.Current.ServiceProvider);
                    filterItem.IsReusable = filterFactory.IsReusable;
                    if (filterItem.Filter == null)
                    {
                        throw new InvalidOperationException($"CreateInstance Fail for {filterFactory.GetType()}");
                    }
                }
            }
        }
    }
}