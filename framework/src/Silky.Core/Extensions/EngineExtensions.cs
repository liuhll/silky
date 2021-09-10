using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Silky.Core.Extensions
{
    public static class EngineExtensions
    {
        public static bool ContainModule([NotNull] this IEngine engine, [NotNull] string moduleName)
        {
            Check.NotNull(engine, nameof(engine));
            Check.NotNull(moduleName, nameof(moduleName));
            return (engine as SilkyEngine)?.Modules.Any(p => p.Name == moduleName) == true;
        }
        
        public static bool IsContainHttpCoreModule([NotNull] this IEngine engine)
        {
            Check.NotNull(engine, nameof(engine));
            return engine.ContainModule("SilkyHttpCore");
        }
    }
}