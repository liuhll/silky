using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IHandleSupervisor : ISingletonDependency
    {
        void Monitor((string, string) item);

        void ExecSuccess((string, string) item, double elapsedTotalMilliseconds);

        void ExecFail((string, string) item, bool isBusinessException, double elapsedTotalMilliseconds);
    }
}