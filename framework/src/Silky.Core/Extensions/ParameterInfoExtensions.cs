using System.Reflection;
using JetBrains.Annotations;
using Silky.Core.Convertible;

namespace Silky.Core.Extensions
{
    public static class ParameterInfoExtensions
    {
        private static ITypeConvertibleService _typeConvertibleService;

        static ParameterInfoExtensions()
        {
            _typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
        }

        public static object GetActualValue([NotNull] this ParameterInfo parameterInfo, object value)
        {
            if (value == null)
            {
                return value;
            }

            return parameterInfo.ParameterType == value.GetType()
                ? value
                : _typeConvertibleService.Convert(value, parameterInfo.ParameterType);
        }
    }
}