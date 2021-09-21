namespace Silky.Rpc.Endpoint
{
    public static class RpcEndpointExtensions
    {
        public static string GetAddress(this IRpcEndpoint rpcEndpoint)
        {
            return $"{rpcEndpoint.Host}:{rpcEndpoint.Port}";
        }
    }
}