using System;

namespace Silky.Core.Modularity.PlugIns;

public interface IPlugInSource
{
    Type[] GetModules();
}