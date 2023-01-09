using System;
using Silky.Core.FilterMetadata;

namespace Silky.Rpc.Filters;

public interface IClientFilterFactory : IClientFilterMetadata
{
    bool IsReusable { get; }
    
    IClientFilterMetadata CreateInstance(IServiceProvider serviceProvider);
}