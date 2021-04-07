using System.Collections.Generic;
using JetBrains.Annotations;

namespace Silky.Lms.Core.Modularity
{
    public interface IModuleContainer
    {
        [NotNull]
        IReadOnlyList<ILmsModuleDescriptor> Modules { get; }
    }
}