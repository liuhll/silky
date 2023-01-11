using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Extensions;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Extensions
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

            if (parameterInfo.ParameterType.IsFormFileType())
            {
                if (parameterInfo.ParameterType.IsFormFileType())
                {
                    if (value is IFormFile)
                    {
                        return value;
                    }

                    if (value is SilkyFormFile file)
                    {
                        return file?.ConventToFormFile();
                    }

                    return value;
                }

                if (parameterInfo.ParameterType.IsMultipleFormFileType())
                {
                    if (value is IFormCollection)
                    {
                        return value;
                    }

                    if (value is SilkyFormFile[] files)
                    {
                        return files?.ConventToFileCollection();
                    }

                    return value;
                }

                if (value is IEnumerable<IFormFile>)
                {
                    return value;
                }

                if (value is IEnumerable<SilkyFormFile> files1)
                {
                    return files1?.ToArray().ConventToFormFiles();
                }
            }

            return parameterInfo.ParameterType == value.GetType()
                ? value
                : _typeConvertibleService.Convert(value, parameterInfo.ParameterType);
        }
    }
}