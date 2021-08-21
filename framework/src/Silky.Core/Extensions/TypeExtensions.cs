using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core.Convertible;

namespace Silky.Core.Extensions
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

            if (typeof(Nullable<>) == type || typeof(Nullable) == type)
            {
                return ObjectDataType.Nullable;
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

        public static MethodInfo GetCompareMethod(this Type type, MethodInfo method, string compareMethodName)

        {
            var methodInfos = type.GetMethods();
            var compareMethods = methodInfos.Where(p => p.Name == compareMethodName);
            MethodInfo compareMethod = null;
            foreach (var methodInfo in compareMethods)
            {
                if (methodInfo.ParameterEquality(method))
                {
                    compareMethod = methodInfo;
                    break;
                }
            }

            return compareMethod;
        }

        /// <summary>
        /// 判断是否是富基元类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static bool IsRichPrimitive(this Type type)
        {
            // 处理元组类型
            if (type.IsValueTuple()) return false;

            // 处理数组类型，基元数组类型也可以是基元类型
            if (type.IsArray) return type.GetElementType().IsRichPrimitive();

            // 基元类型或值类型或字符串类型
            if (type.IsPrimitive || type.IsValueType || type == typeof(string)) return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GenericTypeArguments[0].IsRichPrimitive();

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


        /// <summary>
        /// 判断类型是否实现某个泛型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="generic">泛型类型</param>
        /// <returns>bool</returns>
        public static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            // 检查接口类型
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;

            // 检查类型
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            return false;

            // 判断逻辑
            bool IsTheRawGenericType(Type type) =>
                generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
        }

        /// <summary>
        /// 获取所有祖先类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAncestorTypes(this Type type)
        {
            var ancestorTypes = new List<Type>();
            while (type != null && type != typeof(object))
            {
                if (IsNoObjectBaseType(type))
                {
                    var baseType = type.BaseType;
                    ancestorTypes.Add(baseType);
                    type = baseType;
                }
                else break;
            }

            return ancestorTypes;

            static bool IsNoObjectBaseType(Type type) => type.BaseType != typeof(object);
        }
    }
}