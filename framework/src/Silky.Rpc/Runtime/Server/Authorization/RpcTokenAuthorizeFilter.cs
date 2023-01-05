using Silky.Core.Exceptions;
using Silky.Rpc.Filters;
using Silky.Rpc.Security;

namespace Silky.Rpc.Runtime.Server.Authorization;

public class RpcTokenAuthorizeFilter : IServerAuthorizationFilter
{
    private readonly ITokenValidator _tokenValidator;
    
    public RpcTokenAuthorizeFilter(ITokenValidator tokenValidator)
    {
        _tokenValidator = tokenValidator;
    }

    public void OnAuthorization(ServerAuthorizationFilterContext context)
    {
        if (!_tokenValidator.Validate())
        {
            throw new RpcAuthenticationException("rpc token is illegal");
        }
    }
}