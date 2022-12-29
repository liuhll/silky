using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.MethodExecutor;

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
        
        
        public static object GetActualParameter(this ParameterDescriptor parameterDescriptor, object parameter, IList<IFormFile> silkyFiles)
        {
            var parameterValue = parameterDescriptor.GetActualParameter(parameter);
            if (parameterDescriptor.HasFileProp(out var filePropName))
            {
                parameterValue.GetType().GetProperty(filePropName)
                    .SetValue(parameterValue, silkyFiles.Single(p=> p.Name.Equals(filePropName,StringComparison.OrdinalIgnoreCase)));
            }
            if (parameterDescriptor.HasFilesProp(out var filesPropName))
            {
                var files = new FormFileCollection();
                files.AddRange(silkyFiles);
                parameterValue.GetType().GetProperty(filesPropName)
                    .SetValue(parameterValue, files);
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
                if (typeof(IFormFile).IsAssignableFrom(prop.PropertyType))
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
                if (typeof(IFormFileCollection).IsAssignableFrom(prop.PropertyType))
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
            return typeof(IFormFile).IsAssignableFrom(parameterDescriptor.Type) ||
                   typeof(IFormFileCollection).IsAssignableFrom(parameterDescriptor.Type);
        }

        public static bool IsSupportFileParameter([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.IsFileParameter() || parameterDescriptor.HasFileProp(out var propName);
        }

        public static bool IsSingleFileParameter([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return typeof(IFormFile).IsAssignableFrom(parameterDescriptor.Type);
        }

        public static bool IsMultipleFileParameter([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return typeof(IFormFileCollection).IsAssignableFrom(parameterDescriptor.Type);
        }
        
        public static bool HasFileType([NotNull] this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.ParameterInfo.HasFileType();
        }
        
    }
}