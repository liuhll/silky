using Silky.Core.DependencyInjection;

namespace Silky.Core.MiniProfiler
{
    public interface IMiniProfiler : IScopedDependency
    {
        void Print(string category, string state, string message = null, bool isError = false);
    }
}