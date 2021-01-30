using System;
using JetBrains.Annotations;
using Lms.Core.DependencyInjection;
using Lms.Core.Serialization.Binary;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core.Serialization
{
    public class DefaultObjectSerializer : IObjectSerializer, ITransientDependency
    {
        
        public virtual byte[] Serialize<T>(T obj)
        {
            if (obj == null)
            {
                return null;
            }
            return AutoSerialize(obj);
        }

        [CanBeNull]
        public virtual T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
            {
                return default;
            }
            return AutoDeserialize<T>(bytes);
        }

        public object DeserializeObject(byte[] bytes)
        {
            return BinarySerializationHelper.DeserializeExtended(bytes);
        }
        
        protected virtual byte[] AutoSerialize<T>(T obj)
        {
            return BinarySerializationHelper.Serialize(obj);
        }

        protected virtual T AutoDeserialize<T>(byte[] bytes)
        {
            return (T) BinarySerializationHelper.DeserializeExtended(bytes);
        }
    }
}