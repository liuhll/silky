using System;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IServerFilterFactory : IServerFilterMetadata
{
    bool IsReusable { get; }
    
    IServerFilterMetadata CreateInstance(IServiceProvider serviceProvider);
}