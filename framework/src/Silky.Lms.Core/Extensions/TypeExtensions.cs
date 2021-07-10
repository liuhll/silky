using System;
using Silky.Lms.Core.Convertible;

namespace Silky.Lms.Core.Extensions
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
        
        /// <summary>
        /// 判断是否是富基元类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static bool IsRichPrimitive(this Type type)
        {
            // 处理元组类型
            if (type.IsValueTuple()) return false;

            // 处理数组类型，基元数组类型也可以是基元类型
            if (type.IsArray) return type.GetElementType().IsRichPrimitive();

            // 基元类型或值类型或字符串类型
            if (type.IsPrimitive || type.IsValueType || type == typeof(string)) return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return type.GenericTypeArguments[0].IsRichPrimitive();

            return false;
        }
        
        /// <summary>
        /// 判断是否是元组类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static bool IsValueTuple(this Type type)
        {
            return type.ToString().StartsWith(typeof(ValueTuple).FullName);
        }
    }
    
    
}