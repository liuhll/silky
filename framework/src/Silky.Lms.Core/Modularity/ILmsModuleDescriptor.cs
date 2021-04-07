using System;
using System.Collections.Generic;
using System.Reflection;

namespace Silky.Lms.Core.Modularity
{
    public interface ILmsModuleDescriptor
    {
        Type Type { get; }

        Assembly Assembly { get; }

        ILmsModule Instance { get; }

        string Name { get; }

        IReadOnlyList<ILmsModuleDescriptor> Dependencies { get; }
    }
}