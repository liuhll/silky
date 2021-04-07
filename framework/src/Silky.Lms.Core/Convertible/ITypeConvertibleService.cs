using System;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Core.Convertible
{
    public interface ITypeConvertibleService : ISingletonDependency
    {
         object Convert(object instance, Type conversionType);
        
    }
}