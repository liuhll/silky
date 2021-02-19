namespace Lms.Rpc.Transport.CachingIntercept
{
    public interface ICachingInterceptProvider
    {

        string CacheName { get; }

        string KeyTemplete { get; }
    }
}