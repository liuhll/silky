using System;
using JetBrains.Annotations;

namespace Silky.Lms.Core.Modularity
{
    public interface IDependedTypesProvider
    {
        [NotNull]
        Type[] GetDependedTypes();
    }
}