using Lms.Core.Extensions;
using AspNetHttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;
using DotNettyHttpMethod =DotNetty.Codecs.Http.HttpMethod;

namespace Lms.DotNetty.Protocol.Ws
{
    public static class DotNettyHttpMethodExtensions
    {
        public static AspNetHttpMethod ConvertAspNetCoreHttpMethod(this DotNettyHttpMethod httpMethod)
        {
            return httpMethod.Name.ConventTo<AspNetHttpMethod>();
        }
    }
}