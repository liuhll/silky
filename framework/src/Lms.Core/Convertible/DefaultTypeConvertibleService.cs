using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lms.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Core.Convertible
{
    public class DefaultTypeConvertibleService : ITypeConvertibleService
    {
        private readonly IEnumerable<TypeConvertDelegate> _converters;
        private readonly ILogger<DefaultTypeConvertibleService> _logger;

        public DefaultTypeConvertibleService(IEnumerable<ITypeConvertibleProvider> converterProviders)
        {
            _logger = NullLogger<DefaultTypeConvertibleService>.Instance;
            _logger.LogDebug($"发现了以下类型转换提供程序：{string.Join(",", converterProviders.Select(p => p.ToString()))}。");
            _converters = converterProviders.SelectMany(p=> p.GetConverters());
         
        }

        public object Convert(object instance, Type conversionType)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));
            if (conversionType.GetTypeInfo().IsInstanceOfType(instance))
                return instance;
            object result = null;
            foreach (var converter in _converters)
            {
                result = converter(instance, conversionType);
                if (result != null)
                    break;
            }
            if (result == null)
                throw new LmsException($"无法将实例：{instance}转换为{conversionType}。");
            return result;
        }
        
    }
}