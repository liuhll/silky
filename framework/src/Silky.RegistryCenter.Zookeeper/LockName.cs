namespace Silky.RegistryCenter.Zookeeper
{
    public static class LockName
    {
        public const string RegisterRoute = "RegisterRoute{0}";

        public const string RegisterGateway = "RegisterGateway{0}";

        public const string CreateSubDirectoryIfNotExistAndSubscribeChildrenChange =
            "CreateSubDirectoryIfNotExistAndSubscribeChildrenChange{0}";
        
        public const string CreateGatewaySubDirectoryIfNotExistAndSubscribeChildrenChange =
            "CreateGatewaySubDirectoryIfNotExistAndSubscribeChildrenChange{0}";

        public const string RemoveExceptRoute = "RemoveExceptRoute{0}";
    }
}