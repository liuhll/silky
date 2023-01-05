using System;

namespace Silky.Rpc.Filters;

public interface IServerFilterFactory : IServerFilterMetadata
{
    bool IsReusable { get; }
    
    IFilterMetadata CreateInstance(IServiceProvider serviceProvider);
}