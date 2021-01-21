using System;
using JetBrains.Annotations;

namespace Lms.Core.Modularity
{
    public interface IDependedTypesProvider
    {
        [NotNull]
        Type[] GetDependedTypes();
    }
}