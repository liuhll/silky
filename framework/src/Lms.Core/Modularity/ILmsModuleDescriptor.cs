using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lms.Core.Modularity
{
    public interface ILmsModuleDescriptor
    {
        Type Type { get; }

        Assembly Assembly { get; }

        ILmsModule Instance { get; }
        
        IReadOnlyList<ILmsModuleDescriptor> Dependencies { get; }
    }
}