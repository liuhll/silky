namespace Silky.Rpc.CachingInterceptor.Providers
{
    public interface ICachingInterceptProvider
    {
        string KeyTemplete { get; }

        bool OnlyCurrentUserData { get; set; }

        bool IgnoreMultiTenancy { get; set; }

        CachingMethod CachingMethod { get; }
    }
}