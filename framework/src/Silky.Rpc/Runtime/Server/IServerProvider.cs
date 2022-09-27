namespace Silky.Rpc.Runtime.Server
{
    public interface IServerProvider
    {
        void AddTcpServices();

        void AddHttpServices();

        void AddWsServices();

        IServer GetServer();
    }
}