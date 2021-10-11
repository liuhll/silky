using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Core.Convertible
{
    public interface ITypeConvertibleProvider : ISingletonDependency
    {
        IEnumerable<TypeConvertDelegate> GetConverters();
    }
}