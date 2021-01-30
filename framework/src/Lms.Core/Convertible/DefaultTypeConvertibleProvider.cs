using System;
using System.Collections.Generic;
using System.Reflection;
using Lms.Core.Serialization;

namespace Lms.Core.Convertible
{
    public class DefaultTypeConvertibleProvider : ITypeConvertibleProvider
    {
        private readonly IObjectSerializer _objectSerializer;
        private readonly IJsonSerializer _jsonSerializer;

        public DefaultTypeConvertibleProvider(IObjectSerializer objectSerializer, 
            IJsonSerializer jsonSerializer)
        {
            _objectSerializer = objectSerializer;
            _jsonSerializer = jsonSerializer;
        }


        public IEnumerable<TypeConvertDelegate> GetConverters()
        {
            yield return EnumTypeConvert;
            //简单类型
            yield return SimpleTypeConvert;
            //guid转换器
            yield return GuidTypeConvert;
            
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
            return Enum.Parse(conversionType, instance.ToString(),true);
        }

        private object ComplexTypeConvert(object instance, Type conversionType)
        {
            //return _serializer.Deserialize(instance, conversionType);
            if (instance is byte[])
            {
               return _objectSerializer.DeserializeObject((byte[])instance);
            }
            return _jsonSerializer.Deserialize(conversionType, instance.ToString());
        }
    }
}