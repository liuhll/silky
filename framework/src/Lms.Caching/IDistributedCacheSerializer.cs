using Lms.Core.DependencyInjection;

namespace Lms.Caching
{
    public interface IDistributedCacheSerializer
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] bytes);
    }
}