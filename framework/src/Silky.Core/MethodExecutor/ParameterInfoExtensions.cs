using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Silky.Core.Extensions;

namespace Silky.Core.MethodExecutor
{
    public static class ParameterInfoExtensions
    {
        public static bool IsSampleType(this ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType.IsSample();
        }

        public static bool IsSampleOrNullableType(this ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType.IsSample() || parameterInfo.ParameterType.IsNullableType();
        }

        public static bool IsFormFileType(this ParameterInfo parameterInfo)
        {
            if (parameterInfo.ParameterType == typeof(IFormFileCollection) ||
                parameterInfo.ParameterType == typeof(IFormFile))
            {
                return true;
            }
            var props = parameterInfo.ParameterType.GetProperties();
            var hasFileProp = false;
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(IFormFileCollection) || prop.PropertyType == typeof(IFormFile))
                {
                    hasFileProp = true;

                    break;
                }
            }

            return hasFileProp;
        }

        public static bool IsNullableType(this ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType.IsNullableType();
        }


        [CanBeNull]
        public static object GetDefaultValue(this ParameterInfo parameterInfo)
        {
            ParameterDefaultValue.TryGetDefaultValue(parameterInfo,
                out var parameterDefaultValue);
            return parameterDefaultValue;
        }
    }
}