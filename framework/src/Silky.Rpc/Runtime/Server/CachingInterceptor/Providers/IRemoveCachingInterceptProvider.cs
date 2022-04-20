namespace Silky.Rpc.Runtime.Server;

public interface IRemoveCachingInterceptProvider : ICachingInterceptProvider
{
    string CacheName { get; }
}