using System;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Caching
{
    public interface IDistributedCacheSerializer
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] bytes);

        object Deserialize(byte[] bytes, Type type);
    }
}