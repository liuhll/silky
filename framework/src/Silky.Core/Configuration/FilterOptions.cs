using JetBrains.Annotations;
using Silky.Core.FilterMetadata;

namespace Silky.Core.Configuration;

public class FilterOptions
{
    [NotNull] public ClientFilterList Clients { get; }
    
    [NotNull] public ServerFilterList Servers { get; }

    public FilterOptions()
    {
        Clients = new ClientFilterList();
        Servers = new ServerFilterList();
    }
}