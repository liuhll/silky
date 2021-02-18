namespace Lms.Rpc.Transport.CachingIntercept
{
    public interface ICachingInterceptProvider
    {
        CachingMethod CachingMethod { get; }
        
    }
}