using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Convertible;

namespace Silky.Rpc.Runtime.Server
{
    public static class ParameterDescriptorExtensions
    {
        private static ITypeConvertibleService _typeConvertibleService;

        static ParameterDescriptorExtensions()
        {
            _typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
        }

        public static object GetActualParameter([NotNull] this ParameterDescriptor parameterDescriptor,
            object parameter)
        {
            if (parameterDescriptor.Type.GetType() == parameter.GetType())
            {
                return parameter;
            }

            return _typeConvertibleService.Convert(parameter, parameterDescriptor.Type);
        }
    }
}