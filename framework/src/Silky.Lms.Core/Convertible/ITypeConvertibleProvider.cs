using System.Collections.Generic;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Core.Convertible
{
    public interface ITypeConvertibleProvider: ISingletonDependency
    {
        IEnumerable<TypeConvertDelegate> GetConverters();
    }
}