using System;
using System.Collections.Generic;
using Silky.Core;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Filters
{
    public class DefaultFilterProvider : IFilterProvider, ISingletonDependency
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
                if (filter is IServerFilterFactory serverFilterFactory)
                {
                    filterItem.Filter = serverFilterFactory.CreateInstance(EngineContext.Current.ServiceProvider);
                    filterItem.IsReusable = serverFilterFactory.IsReusable;
                    if (filterItem.Filter == null)
                    {
                        throw new InvalidOperationException($"CreateInstance Server Filter Fail  for {serverFilterFactory.GetType()}");
                    }
                   
                }
                else if (filter is IClientFilterFactory clientFilterFactory)
                {
                    filterItem.Filter = clientFilterFactory.CreateInstance(EngineContext.Current.ServiceProvider);
                    filterItem.IsReusable = clientFilterFactory.IsReusable;
                    if (filterItem.Filter == null)
                    {
                        throw new InvalidOperationException($"CreateInstance Client Filter Fail for {clientFilterFactory.GetType()}");
                    }
                }
                else
                {
                    filterItem.Filter = filter;
                    filterItem.IsReusable = true;
                }
            }
        }
    }
}