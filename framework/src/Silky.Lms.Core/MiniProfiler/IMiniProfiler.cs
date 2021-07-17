using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Core
{
    public interface IMiniProfiler : IScopedDependency
    {
        void Print(string category, string state, string message = null, bool isError = false);
    }
}