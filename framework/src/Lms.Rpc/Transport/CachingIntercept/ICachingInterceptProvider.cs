namespace Lms.Rpc.Transport.CachingIntercept
{
    public interface ICachingInterceptProvider
    {
        
        string KeyTemplete { get; }

        bool OnlyCurrentUserData { get; set; }
    }
}