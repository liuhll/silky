using System;
using System.Collections.Generic;
using System.Linq;
using Silky.Core;
using Silky.Core.Reflection;

namespace Silky.Rpc.Extensions;

public static class TypeFinderExtensions
{
    public static IEnumerable<Type> GetaAllExportedTypes(this ITypeFinder typeFinder)
    {
        var types = typeFinder.GetAssemblies()
            .SelectMany(p =>
            {
                try
                {
                    return p.ExportedTypes;
                }
                catch //Entity Framework 6 doesn't allow getting types (throws an exception)
                {
                    return new List<Type>();
                }
            });
        return types;
    }
}