namespace Silky.Rpc.Endpoint
{
    public static class SilkyEndpointExtensions
    {
        public static string GetAddress(this ISilkyEndpoint silkyEndpoint)
        {
            return $"{silkyEndpoint.Host}:{silkyEndpoint.Port}";
        }
        
    }
}