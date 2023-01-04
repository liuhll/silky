namespace Silky.Rpc.Filters;

public interface IOrderedServerFilter : IServerFilterMetadata
{
    int Order { get; }
}