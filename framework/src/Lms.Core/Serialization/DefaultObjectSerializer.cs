using System;
using JetBrains.Annotations;
using Lms.Core.DependencyInjection;
using Lms.Core.Serialization.Binary;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Core.Serialization
{
    public class DefaultObjectSerializer : IObjectSerializer, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultObjectSerializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public virtual byte[] Serialize<T>(T obj)
        {
            if (obj == null)
            {
                return null;
            }

            //Check if a specific serializer is registered
            using (var scope = _serviceProvider.CreateScope())
            {
                var specificSerializer = scope.ServiceProvider.GetService<IObjectSerializer<T>>();
                if (specificSerializer != null)
                {
                    return specificSerializer.Serialize(obj);
                }
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

            //Check if a specific serializer is registered
            using (var scope = _serviceProvider.CreateScope())
            {
                var specificSerializer = scope.ServiceProvider.GetService<IObjectSerializer<T>>();
                if (specificSerializer != null)
                {
                    return specificSerializer.Deserialize(bytes);
                }
            }

            return AutoDeserialize<T>(bytes);
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