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
            if (serviceEntryDescriptor == null)
            {
                return false;
            }
            return isEnabled && !serviceEntryDescriptor.GetMetadata<bool>(ServiceEntryConstant.DisableAuditing);
        }

        public static bool IsUnWrapperResult(this ServiceEntryDescriptor serviceEntryDescriptor)
        {
            if (serviceEntryDescriptor == null)
            {
                return false;
            }

            return serviceEntryDescriptor.GetMetadata<bool>(ServiceEntryConstant.UnWrapperResult);
        }
    }
}