using System;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Convertible
{
    public interface ITypeConvertibleService : ISingletonDependency
    {
        object Convert(object instance, Type conversionType);
    }
}