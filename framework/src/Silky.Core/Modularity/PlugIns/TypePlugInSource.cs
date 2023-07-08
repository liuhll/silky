using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core.Exceptions;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Core.Modularity.PlugIns;

public class TypePlugInSource : IPlugInSource
{
    private readonly Type[] _moduleTypes;

    private readonly string[] _moduleTypeNames;


    public string[] ModuleTypeNames => _moduleTypeNames;
    

    public TypePlugInSource(params string[] moduleTypeNames)
    {
        _moduleTypeNames = moduleTypeNames;
    }

    public TypePlugInSource(params Type[] moduleTypes)
    {
        _moduleTypes = moduleTypes;
    }

    private Type[] LoadModuleTypes(params string[] moduleTypeNames)
    {
        var moduleTypes = new List<Type>();
        foreach (var moduleTypeName in moduleTypeNames)
        {
            if (!moduleTypeNames.IsNullOrEmpty())
            {
                var type = Type.GetType(moduleTypeName);
                if (type == null)
                {
                    throw new SilkyException($"Cannot load plugin of {moduleTypeName} Type");
                }

                if (!SilkyModule.IsSilkyPluginModule(type))
                {
                    throw new SilkyException(type.FullName + "is not a module type ");
                }

                moduleTypes.AddIfNotContains(type);
            }
        }

        return moduleTypes.ToArray();
    }


    public Type[] GetModules()
    {
        var moduleTypes = new List<Type>();
        if (_moduleTypeNames?.Any() == true)
        {
            moduleTypes.AddRange(LoadModuleTypes(_moduleTypeNames));
        }

        if (_moduleTypes?.Any() == true)
        {
            moduleTypes.AddRange(_moduleTypes);
        }

        return moduleTypes.ToArray();
    }
}