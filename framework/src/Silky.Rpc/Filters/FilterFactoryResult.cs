namespace Silky.Rpc.Filters;

internal readonly struct FilterFactoryResult
{
    public FilterFactoryResult(
        FilterItem[] cacheableFilters,
        IServerFilterMetadata[] filters)
    {
        CacheableFilters = cacheableFilters;
        Filters = filters;
    }

    public FilterItem[] CacheableFilters { get; }

    public IServerFilterMetadata[] Filters { get; }
}