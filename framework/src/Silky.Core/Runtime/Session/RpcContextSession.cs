using System.Security.Claims;
using Silky.Core.Runtime.Rpc;

namespace Silky.Core.Runtime.Session
{
    public class RpcContextSession : SessionBase
    {
        internal RpcContextSession()
        {
        }

        public override object? UserId
        {
            get
            {
                var userId = RpcContext.Context.GetInvokeAttachment(ClaimTypes.NameIdentifier);
                return userId;
            }
        }

        public override string UserName
        {
            get
            {
                var userName = RpcContext.Context.GetInvokeAttachment(ClaimTypes.Name);
                if (userName != null)
                {
                    return userName.ToString();
                }

                return null;
            }
        }

        public override object TenantId
        {
            get
            {
                var tenantId = RpcContext.Context.GetInvokeAttachment(SilkyClaimTypes.TenantId);
                return tenantId;
            }
        }
    }
}