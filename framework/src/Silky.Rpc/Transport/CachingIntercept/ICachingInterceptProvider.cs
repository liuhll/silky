namespace Silky.Rpc.Transport.CachingIntercept
{
    public interface ICachingInterceptProvider
    {
        
        string KeyTemplete { get; }

        bool OnlyCurrentUserData { get; set; }
        
        CachingMethod CachingMethod { get; }
    }
}