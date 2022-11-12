namespace Silky.Rpc.Runtime.Server;

public interface IUpdateCachingInterceptProvider : ICachingInterceptProvider
{
    public bool IgnoreWhenCacheKeyNull { get; set; }
}