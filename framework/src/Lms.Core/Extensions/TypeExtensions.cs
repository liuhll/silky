using System;
using Lms.Core.Convertible;

namespace Lms.Core.Extensions
{
    public static class TypeExtensions
    {
        public static ObjectDataType GetObjectDataType(this Type type)
        {
            if (type.IsEnum)
            {
                return ObjectDataType.Enum;
            }

            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return ObjectDataType.Convertible;
            }

            if (type == typeof(Guid))
            {
                return ObjectDataType.Guid;
            }

            return ObjectDataType.Complex;
        }
        
        public static bool IsSample(this Type type)
        {
            var objectType = type.GetObjectDataType();
            return objectType != ObjectDataType.Complex;
        }
    }
    
    
}