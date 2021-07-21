using System;
using JetBrains.Annotations;

namespace Silky.Core.Modularity
{
    public interface IDependedTypesProvider
    {
        [NotNull]
        Type[] GetDependedTypes();
    }
}