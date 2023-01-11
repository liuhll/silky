using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;

namespace Silky.Rpc.Runtime.Server
{
    public static class RpcParameterExtensions
    {
        private static ITypeConvertibleService _typeConvertibleService;

        static RpcParameterExtensions()
        {
            _typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
        }

        public static object GetActualParameter([NotNull] this RpcParameter rpcParameter,
            object parameter)
        {
            if (parameter == null)
            {
                return null;
            }

            if (parameter is JObject jObjectParameter)
            {
                var rpcParameterProperties = rpcParameter.Type.GetProperties();
                foreach (var item in jObjectParameter)
                {
                    var rpcParameterProperty = rpcParameterProperties.SingleOrDefault(p =>
                        p.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    if (rpcParameterProperty == null)
                    {
                        continue;
                    }
                    if (rpcParameterProperty.PropertyType.IsEnumerable())
                    {
                        jObjectParameter[item.Key] =
                            JToken.Parse($"[{item.Value}]");
                    }
                }
            }
            if (rpcParameter.Type.IsEnumerable() && parameter.GetType() == typeof(string) && !parameter.ToString().IsValidJson())
            {
                parameter = $"[{parameter}]";
            }
            
            return rpcParameter.Type?.GetType() == parameter.GetType()
                ? parameter
                : _typeConvertibleService.Convert(parameter, rpcParameter.Type);
        }

        public static object GetActualParameter([NotNull] this RpcParameter rpcParameter,
            object parameter, HttpContext context)
        {
            var parameterValue = rpcParameter.GetActualParameter(parameter);
            if (rpcParameter.HasFileProp(out var filePropName))
            {
                parameterValue.GetType().GetProperty(filePropName)
                    .SetValue(parameterValue, context.Request.Form.Files.GetFile(filePropName));
            }

            if (rpcParameter.HasFilesProp(out var filesPropName))
            {
                parameterValue.GetType().GetProperty(filesPropName)
                    .SetValue(parameterValue, context.Request.Form.Files);
            }

            return parameterValue;
        }


        public static object GetActualParameter(this RpcParameter rpcParameter, object parameter,
            IList<IFormFile> silkyFiles)
        {
            var parameterValue = rpcParameter.GetActualParameter(parameter);
            if (rpcParameter.HasFileProp(out var filePropName))
            {
                parameterValue.GetType().GetProperty(filePropName)
                    .SetValue(parameterValue,
                        silkyFiles.Single(p => p.Name.Equals(filePropName, StringComparison.OrdinalIgnoreCase)));
            }

            if (rpcParameter.HasFilesProp(out var filesPropName))
            {
                var files = new FormFileCollection();
                files.AddRange(silkyFiles);
                parameterValue.GetType().GetProperty(filesPropName)
                    .SetValue(parameterValue, files);
            }

            return parameterValue;
        }

        private static bool HasFileProp([NotNull] this RpcParameter rpcParameter, out string filePropName)
        {
            var props = rpcParameter.Type.GetProperties();
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

        private static bool HasFilesProp([NotNull] this RpcParameter rpcParameter,
            out string filesPropName)
        {
            var props = rpcParameter.Type.GetProperties();
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

        public static bool IsFileParameter([NotNull] this RpcParameter rpcParameter)
        {
            return typeof(IFormFile).IsAssignableFrom(rpcParameter.Type) ||
                   typeof(IFormFileCollection).IsAssignableFrom(rpcParameter.Type);
        }

        public static bool IsSupportFileParameter([NotNull] this RpcParameter rpcParameter)
        {
            return rpcParameter.IsFileParameter() || rpcParameter.HasFileProp(out var propName);
        }

        public static bool IsSingleFileParameter([NotNull] this RpcParameter rpcParameter)
        {
            return typeof(IFormFile).IsAssignableFrom(rpcParameter.Type);
        }

        public static bool IsMultipleFileParameter([NotNull] this RpcParameter rpcParameter)
        {
            return typeof(IFormFileCollection).IsAssignableFrom(rpcParameter.Type);
        }

        public static bool HasFileType([NotNull] this RpcParameter rpcParameter)
        {
            return rpcParameter.ParameterInfo.HasFileType();
        }
    }
}