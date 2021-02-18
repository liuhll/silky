namespace Lms.Rpc.Transport.CachingIntercept
{
    public interface IUpdateCachingInterceptProvider : ICachingInterceptProvider
    {
        public string[] CacheName { get; }
    }
}