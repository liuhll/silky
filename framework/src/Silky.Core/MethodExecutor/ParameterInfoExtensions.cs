using System.Reflection;
using JetBrains.Annotations;
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