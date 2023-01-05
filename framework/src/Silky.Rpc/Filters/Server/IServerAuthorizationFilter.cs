namespace Silky.Rpc.Filters;

public interface IServerAuthorizationFilter : IServerFilterMetadata
{
    void OnAuthorization(ServerAuthorizationFilterContext context);
}