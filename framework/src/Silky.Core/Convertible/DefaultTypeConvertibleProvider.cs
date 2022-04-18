using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core.Serialization;

namespace Silky.Core.Convertible
{
    public class DefaultTypeConvertibleProvider : ITypeConvertibleProvider
    {
        private readonly ISerializer _serializer;

        public DefaultTypeConvertibleProvider(ISerializer serializer)
        {
            _serializer = serializer;
        }


        public IEnumerable<TypeConvertDelegate> GetConverters()
        {
            // Enum type converter
            yield return EnumTypeConvert;
            // Simple type converter
            yield return SimpleTypeConvert;
            // Guid type converter
            yield return GuidTypeConvert;
            // Complex type converter
            yield return ComplexTypeConvert;
        }

        private object GuidTypeConvert(object instance, Type conversionType)
        {
            if (instance == null || conversionType != typeof(Guid))
                return null;
            Guid.TryParse(instance.ToString(), out Guid result);
            return result;
        }

        private object SimpleTypeConvert(object instance, Type conversionType)
        {
            if (instance is IConvertible && typeof(IConvertible).IsAssignableFrom(conversionType))
                return Convert.ChangeType(instance, conversionType);
            return null;
        }

        private object EnumTypeConvert(object instance, Type conversionType)
        {
            if (instance == null || !conversionType.GetTypeInfo().IsEnum)
                return null;
            return Enum.Parse(conversionType, instance.ToString(), true);
        }

        private object ComplexTypeConvert(object instance, Type conversionType)
        {
            if (instance.GetType().GetInterfaces().Any(p => p == typeof(IDictionary)))
            {
                instance = _serializer.Serialize(instance);
            }
            else if (instance.GetType() != typeof(string))
            {
                instance = _serializer.Serialize(instance);
            }

            return _serializer.Deserialize(conversionType, instance.ToString());
        }
    }
}