namespace Lms.Rpc.Transport.CachingIntercept
{
    public interface IRemoveCachingInterceptProvider
    {
        
        string CacheName { get; }

        string KeyTemplete { get; }
    }
}