using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Silky.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplay(this Enum value)
        {
            var desc = value.GetAttribute<DescriptionAttribute>();
            if (desc != null)
            {
                return desc.Description;
            }

            var display = value.GetAttribute<DisplayAttribute>();
            if (display != null)
            {
                return !display.Description.IsNullOrEmpty() ? display.Description : display.Name;
            }

            return "";
        }

        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            return field.GetCustomAttribute(typeof(T)) as T;
        }

        public static int GetValue(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        public static IDictionary<int, string> GetEnumSources(this Type type)
        {
            if (!type.GetTypeInfo().IsEnum)
                throw new Exception("type 类型必须为枚举类型!");

            var dict = new Dictionary<int, string>();


            foreach (var value in Enum.GetValues(type))
            {
                var fieldName = Enum.GetName(type, value);
                var field = type.GetField(fieldName);
                var display = field.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                dict.Add(Convert.ToInt32(value), display != null ? display.Description : fieldName);
            }

            return dict;
        }

        public static IDictionary<T, string> GetEnumSources<T>(this Type type) where T : Enum
        {
            if (!type.GetTypeInfo().IsEnum)
                throw new Exception("type 类型必须为枚举类型!");

            var dict = new Dictionary<T, string>();

            foreach (var value in Enum.GetValues(type))
            {
                var fieldName = Enum.GetName(type, value);
                var field = type.GetField(fieldName);
                var display = field.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                dict.Add(Convert.ToInt32(value).ToString().ConventTo<T>(),
                    display != null ? display.Description : fieldName);
            }

            return dict;
        }

        // public static List<Tuple<string, string>> GetEnumSources(this Enum value)
        // {
        //     var type = value.GetType();
        //     if (!type.GetTypeInfo().IsEnum)
        //         throw new Exception("type 类型必须为枚举类型!");
        //
        //     var list = new List<Tuple<string, string>>();
        //
        //     foreach (var value in Enum.GetValues(type))
        //     {
        //         var fieldName = Enum.GetName(type, value);
        //         var field = type.GetField(fieldName);
        //         var display = field.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
        //         if (display != null)
        //             list.Add(new Tuple<string, string>(Convert.ToInt32(value) + "", display.Name));
        //         else
        //             list.Add(new Tuple<string, string>(Convert.ToInt32(value) + "", fieldName));
        //     }
        //     return list;
        // }
    }
}