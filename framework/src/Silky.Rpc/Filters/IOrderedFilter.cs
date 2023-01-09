using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IOrderedFilter : IFilterMetadata
{
    int Order { get; }
}