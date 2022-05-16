using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
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
            if (parameter == null)
            {
                return null;
            }

            return parameterDescriptor.Type?.GetType() == parameter.GetType()
                ? parameter
                : _typeConvertibleService.Convert(parameter, parameterDescriptor.Type);
        }

        public static object GetActualParameter([NotNull] this ParameterDescriptor parameterDescriptor,
            object parameter, HttpContext context)
        {
            var parameterValue = parameterDescriptor.GetActualParameter(parameter);
            if (parameterDescriptor.HasFileProp(out var filePropName))
            {
                parameterValue.GetType().GetProperty(filePropName)
                    .SetValue(parameterValue, context.Request.Form.Files.GetFile(filePropName));
            }

            if (parameterDescriptor.HasFilesProp(out var filesPropName))
            {
                parameterValue.GetType().GetProperty(filesPropName)
                    .SetValue(parameterValue, context.Request.Form.Files);
            }

            return parameterValue;
        }

        private static bool HasFileProp([NotNull] this ParameterDescriptor parameterDescriptor, out string filePropName)
        {
            var props = parameterDescriptor.Type.GetProperties();
            var hasFileProp = false;
            filePropName = null;
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(IFormFile))
                {
                    hasFileProp = true;
                    filePropName = prop.Name;
                    break;
                }
            }

            return hasFileProp;
        }

        private static bool HasFilesProp([NotNull] this ParameterDescriptor parameterDescriptor,
            out string filesPropName)
        {
            var props = parameterDescriptor.Type.GetProperties();
            var hasFileProp = false;
            filesPropName = null;
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(IFormFileCollection))
                {
                    hasFileProp = true;
                    filesPropName = prop.Name;
                    break;
                }
            }

            return hasFileProp;
        }

        public static bool IsFileParameter([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.Type == typeof(IFormFile) ||
                   parameterDescriptor.Type == typeof(IFormFileCollection);
        }

        public static bool IsSupportFileParameter([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.IsFileParameter() || parameterDescriptor.HasFileProp(out var propName);
        }

        public static bool IsSingleFileParameter([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.Type == typeof(IFormFile);
        }

        public static bool IsMultipleFileParameter([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.Type == typeof(IFormFileCollection);
        }
    }
}