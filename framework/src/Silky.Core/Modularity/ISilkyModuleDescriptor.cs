using System;
using System.Collections.Generic;
using System.Reflection;

namespace Silky.Core.Modularity
{
    public interface ISilkyModuleDescriptor
    {
        Type Type { get; }

        Assembly Assembly { get; }

        ISilkyModule Instance { get; }

        string Name { get; }

        IReadOnlyList<ISilkyModuleDescriptor> Dependencies { get; }
    }
}