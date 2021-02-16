using System.Reflection;
using JetBrains.Annotations;
using Lms.Core.Extensions;

namespace Lms.Rpc.Runtime.Server.Parameter
{
    public static class ParameterInfoExtensions
    {
        public static bool IsSampleType(this ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType.IsSample();
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