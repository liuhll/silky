using Silky.Lms.Core.Extensions;

namespace Silky.Lms.Rpc.Transport
{
    public static class RpcContextExtensions
    {
        public static bool IsGateway(this RpcContext rpcContext)
        {
            var isGateway = RpcContext.GetContext().GetAttachment(AttachmentKeys.IsGatewayHost);
            if (isGateway == null)
            {
                return false;
            }

            return isGateway.To<bool>();
        }
    }
}