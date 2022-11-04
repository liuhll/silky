namespace Silky.Rpc.Runtime.Server
{
    public interface IServerProvider
    {
        void AddRpcServices();

        void AddHttpServices();

        void AddWsServices();

        IServer GetServer();
    }
}