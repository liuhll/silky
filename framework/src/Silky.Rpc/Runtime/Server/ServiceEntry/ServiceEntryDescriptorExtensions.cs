namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceEntryDescriptorExtensions
    {
        public static string GetAuthor(this ServiceEntryDescriptor serviceEntryDescriptor)
        {
            return serviceEntryDescriptor.GetMetadata<string>(ServiceConstant.AuthorKey);
        }
    }
}