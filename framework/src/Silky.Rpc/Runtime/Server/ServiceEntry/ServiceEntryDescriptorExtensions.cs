namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceEntryDescriptorExtensions
    {
        public static string GetAuthor(this ServiceEntryDescriptor serviceEntryDescriptor)
        {
            return serviceEntryDescriptor.GetMetadata<string>(ServiceConstant.AuthorKey);
        }

        public static bool IsEnableAuditing(this ServiceEntryDescriptor serviceEntryDescriptor, bool isEnabled)
        {
            return isEnabled && serviceEntryDescriptor.GetMetadata<bool>(ServiceEntryConstant.DisableAuditing);
        }
    }
}