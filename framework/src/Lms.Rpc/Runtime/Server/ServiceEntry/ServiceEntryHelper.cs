using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lms.Core;
using Lms.Rpc.Runtime.Server.ServiceEntry.ServiceDiscovery;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    internal class ServiceEntryHelper
    {
        public static IEnumerable<Type> FindServiceEntryTypes(ITypeFinder typeFinder)
        {
            var types = typeFinder.GetAssemblies()
                    .SelectMany(p => p.ExportedTypes)
                    .Where(p=> p.IsClass
                               && !p.IsAbstract
                               && p.GetInterfaces().Any(i=> i.GetCustomAttributes().Any(a=> a is ServiceBundleAttribute))
                    ).SelectMany(p=> p.GetInterfaces().Where(i=> i.GetCustomAttributes().Any(a=> a is ServiceBundleAttribute)))
                ;
            return types;
        }

        public static IEnumerable<Type> FindAllServiceEntryTypes(ITypeFinder typeFinder)
        {
            var types = typeFinder.GetAssemblies()
                    .SelectMany(p => p.ExportedTypes)
                    .Where(p=> p.IsInterface 
                               && p.GetCustomAttributes().Any(a=> a is ServiceBundleAttribute)
                    )
                ;
            return types;
        }
        
    }
}