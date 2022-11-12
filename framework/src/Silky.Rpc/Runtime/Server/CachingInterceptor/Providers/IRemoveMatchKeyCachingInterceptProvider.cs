namespace Silky.Rpc.Runtime.Server;

public interface IRemoveMatchKeyCachingInterceptProvider : ICachingInterceptProvider
{
     string CacheName { get; set; }
}