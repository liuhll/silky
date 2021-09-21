using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Routing
{
    public interface IServerRegisterProvider : ISingletonDependency
    {
        void AddTcpServices();

        void AddHttpServices();

        void AddWsServices();

        ServerRoute GetServerRoute();
    }
}