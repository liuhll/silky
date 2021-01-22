using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lms.Core.Modularity
{
    public interface IModuleContainer
    {
        [NotNull]
        IReadOnlyList<ILmsModuleDescriptor> Modules { get; }
    }
}