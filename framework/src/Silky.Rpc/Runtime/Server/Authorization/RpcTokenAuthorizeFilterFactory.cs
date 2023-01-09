using Microsoft.Extensions.DependencyInjection;
using Silky.Core.DependencyInjection;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Filters;
using Silky.Rpc.Security;

namespace Silky.Rpc.Runtime.Server.Authorization;

public class RpcTokenAuthorizeFilterFactory : IServerFilterFactory, ISingletonDependency
{
    public bool IsReusable { get; } = true;
    public IServerFilterMetadata CreateInstance(System.IServiceProvider serviceProvider)
    {
        return new RpcTokenAuthorizeFilter(serviceProvider.GetRequiredService<ITokenValidator>());
    }
    
}