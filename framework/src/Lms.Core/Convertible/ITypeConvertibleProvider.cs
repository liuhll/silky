using System.Collections.Generic;
using Lms.Core.DependencyInjection;

namespace Lms.Core.Convertible
{
    public interface ITypeConvertibleProvider: ISingletonDependency
    {
        IEnumerable<TypeConvertDelegate> GetConverters();
    }
}