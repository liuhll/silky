namespace Silky.Rpc.Runtime.Server.Descriptor
{
    public static class ServiceDescriptorExtensions
    {
        public static string GetAuthor(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.GetMetadata<string>(ServiceEntryConstant.AuthorKey);
        }

        public static string GetWsPath(this ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.GetMetadata<string>(ServiceEntryConstant.WsPath);
        }
    }
}