using System;
using Lms.Core.DependencyInjection;

namespace Lms.Core.Convertible
{
    public interface ITypeConvertibleService : ISingletonDependency
    {
         object Convert(object instance, Type conversionType);
        
    }
}