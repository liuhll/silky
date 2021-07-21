using System;
using MessagePack;
using MessagePack.ReactivePropertyExtension;
using MessagePack.Resolvers;

namespace Silky.Codec
{
    public class SerializerUtilitys
    {
        static SerializerUtilitys()
        {
            // Set extensions to default resolver.
            var resolver = CompositeResolver.Create(
                // enable extension packages first
                ReactivePropertyResolver.Instance,
                TypelessObjectResolver.Instance,
                DynamicObjectResolver.Instance,
                TypelessContractlessStandardResolver.Instance,
                // finally use standard (default) resolve
                StandardResolver.Instance
            );
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            MessagePackSerializer.DefaultOptions = options;
        }

        public static byte[] Serialize<T>(T instance)
        {
            return MessagePackSerializer.Serialize(instance);
        }

        public static byte[] Serialize(object instance, Type type)
        {
            return MessagePackSerializer.Serialize(instance);
        }

        public static object Deserialize(byte[] data, Type type)
        {
            return data == null ? null : MessagePackSerializer.Deserialize(type, data);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return data == null ? default(T) : MessagePackSerializer.Deserialize<T>(data);
        }
    }
}