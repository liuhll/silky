using JetBrains.Annotations;

namespace Silky.Rpc.Runtime.Server
{
    public class AuthorAttribute : MetadataAttribute
    {
        public AuthorAttribute([NotNull] string name) : base(ServiceConstant.AuthorKey, name)
        {
        }
    }
}