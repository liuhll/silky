using JetBrains.Annotations;

namespace Silky.Rpc.Runtime.Server.ServiceDiscovery
{
    public class AuthorAttribute : MetadataAttribute
    {
        public AuthorAttribute([NotNull] string name) : base(ServiceEntryConstant.AuthorKey, name)
        {
        }
    }
}