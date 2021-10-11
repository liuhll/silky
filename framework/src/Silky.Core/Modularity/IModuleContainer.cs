using System.Collections.Generic;
using JetBrains.Annotations;

namespace Silky.Core.Modularity
{
    public interface IModuleContainer
    {
        [NotNull] IReadOnlyList<ISilkyModuleDescriptor> Modules { get; }
    }
}