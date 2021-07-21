namespace Silky.Rpc.Transport.CachingIntercept
{
    public interface IRemoveCachingInterceptProvider : ICachingInterceptProvider
    {
        string CacheName { get; }
    }
}