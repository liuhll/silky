using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Rpc
{
    public interface IMiniProfiler : IScopedDependency
    {
        void Print(string category, string state, string message = null, bool isError = false);
    }
}