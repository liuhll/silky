namespace Silky.Rpc.Filters;

internal readonly struct FilterFactoryResult
{
    public FilterFactoryResult(
        FilterItem[] cacheableFilters,
        IFilterMetadata[] filters)
    {
        CacheableFilters = cacheableFilters;
        Filters = filters;
    }

    public FilterItem[] CacheableFilters { get; }

    public IFilterMetadata[] Filters { get; }
}