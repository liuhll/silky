using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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