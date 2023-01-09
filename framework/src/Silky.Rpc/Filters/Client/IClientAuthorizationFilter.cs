namespace Silky.Rpc.Filters;

public interface IClientAuthorizationFilter : IClientFilterMetadata
{
    void OnAuthorization(ClientAuthorizationFilterContext context);
}