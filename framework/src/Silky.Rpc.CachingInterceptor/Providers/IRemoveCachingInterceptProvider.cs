namespace Silky.Rpc.CachingInterceptor.Providers
{
    public interface IRemoveCachingInterceptProvider : ICachingInterceptProvider
    {
        string CacheName { get; }
    }
}