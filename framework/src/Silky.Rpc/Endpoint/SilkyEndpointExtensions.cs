namespace Silky.Rpc.Endpoint
{
    public static class SilkyEndpointExtensions
    {
        public static string GetAddress(this ISilkyEndpoint silkyEndpoint)
        {
            return $"{silkyEndpoint.Host}:{silkyEndpoint.Port}";
        }
        
        public static string GetUri(this ISilkyEndpoint silkyEndpoint)
        {
            return $"{silkyEndpoint.ServiceProtocol}://{silkyEndpoint.Host}:{silkyEndpoint.Port}".ToLower();
        }
        
    }
}