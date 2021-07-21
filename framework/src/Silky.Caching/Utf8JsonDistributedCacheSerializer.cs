using System;
using System.Text;
using Silky.Core.DependencyInjection;
using Silky.Core.Serialization;

namespace Silky.Caching
{
    public class Utf8JsonDistributedCacheSerializer : IDistributedCacheSerializer, ITransientDependency
    {
        protected ISerializer Jerializer { get; }

        public Utf8JsonDistributedCacheSerializer(ISerializer serializer)
        {
            Jerializer = serializer;
        }

        public byte[] Serialize<T>(T obj)
        {
            return Encoding.UTF8.GetBytes(Jerializer.Serialize(obj));
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return (T)Jerializer.Deserialize(typeof(T), Encoding.UTF8.GetString(bytes));
        }

        public object Deserialize(byte[] bytes, Type type)
        {
            return Jerializer.Deserialize(type, Encoding.UTF8.GetString(bytes));
        }
    }
}