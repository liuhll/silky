namespace Silky.Rpc.Runtime.Server;

public interface ICachingInterceptProvider
{
    string KeyTemplete { get; }

    bool OnlyCurrentUserData { get; set; }

    bool IgnoreMultiTenancy { get; set; }

    CachingMethod CachingMethod { get; }
}