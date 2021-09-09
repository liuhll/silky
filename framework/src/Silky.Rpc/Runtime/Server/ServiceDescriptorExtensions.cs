namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceDescriptorExtensions
    {
        public static string GetAuthor(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.GetMetadata<string>(ServiceConstant.AuthorKey);
        }

        public static string GetWsPath(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.GetMetadata<string>(ServiceConstant.WsPath);
        }
    }
}