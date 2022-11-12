namespace Silky.Rpc.Runtime.Server;

public interface ICachingInterceptProvider
{
    string KeyTemplate { get; }

    bool OnlyCurrentUserData { get; set; }

    bool IgnoreMultiTenancy { get; set; }

    CachingMethod CachingMethod { get; }

    public CachingInterceptorDescriptor CachingInterceptorDescriptor { get; internal set; }
}