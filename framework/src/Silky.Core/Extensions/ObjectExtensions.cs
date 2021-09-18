using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using Silky.Core.Convertible;

namespace Silky.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static T As<T>(this object obj)
            where T : class
        {
            return (T)obj;
        }

        public static T To<T>(this object obj)
            where T : struct
        {
            if (typeof(T) == typeof(Guid))
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(obj.ToString());
            }

            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), obj?.ToString(), true);
            }

            return (T)Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
        }

        public static T ConventTo<T>(this object instance)
        {
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            return (T)typeConvertibleService.Convert(instance, typeof(T));
        }

        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <param name="obj">待转换的对象</param>
        /// <param name="type">目标类型</param>
        /// <returns>转换后的对象</returns>
        public static object ChangeType(this object obj, Type type)
        {
            if (type == null) return obj;
            if (type == typeof(string)) return obj?.ToString();
            if (type == typeof(Guid) && obj != null) return Guid.Parse(obj.ToString());
            if (obj == null) return type.IsValueType ? Activator.CreateInstance(type) : null;

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (type.IsAssignableFrom(obj.GetType())) return obj;
            else if ((underlyingType ?? type).IsEnum)
            {
                if (underlyingType != null && string.IsNullOrWhiteSpace(obj.ToString())) return null;
                else return Enum.Parse(underlyingType ?? type, obj.ToString());
            }
            // 处理DateTime -> DateTimeOffset 类型
            else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateTimeOffset)))
            {
                return ((DateTime)obj).ConvertToDateTimeOffset();
            }
            // 处理 DateTimeOffset -> DateTime 类型
            else if (obj.GetType().Equals(typeof(DateTimeOffset)) && (underlyingType ?? type).Equals(typeof(DateTime)))
            {
                return ((DateTimeOffset)obj).ConvertToDateTime();
            }
            else if (typeof(IConvertible).IsAssignableFrom(underlyingType ?? type))
            {
                try
                {
                    return Convert.ChangeType(obj, underlyingType ?? type, null);
                }
                catch
                {
                    return underlyingType == null ? Activator.CreateInstance(type) : null;
                }
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(obj.GetType())) return converter.ConvertFrom(obj);

                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    var o = constructor.Invoke(null);
                    var propertys = type.GetProperties();
                    var oldType = obj.GetType();

                    foreach (var property in propertys)
                    {
                        var p = oldType.GetProperty(property.Name);
                        if (property.CanWrite && p != null && p.CanRead)
                        {
                            property.SetValue(o, ChangeType(p.GetValue(obj, null), property.PropertyType), null);
                        }
                    }

                    return o;
                }
            }

            return obj;
        }

        public static T ChangeType<T>(this object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }

        public static Type GetActualType(this object obj)
        {
            if (obj == null) return default;

            var objType = obj.GetType();

            if (obj is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.Null => typeof(string),
                    JsonValueKind.String => typeof(string),
                    JsonValueKind.Number => typeof(long),
                    JsonValueKind.True => typeof(bool),
                    JsonValueKind.False => typeof(bool),
                    _ => typeof(string)
                };
            }

            return objType;
        }
    }
}