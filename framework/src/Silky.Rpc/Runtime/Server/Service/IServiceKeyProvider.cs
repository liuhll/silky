namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceKeyProvider
    {
        string Name { get; }

        int Weight { get; }
    }
}