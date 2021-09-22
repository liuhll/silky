using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerProvider : ISingletonDependency
    {
        void AddTcpServices();

        void AddHttpServices();

        void AddWsServices();

        IServer GetServer();
    }
}