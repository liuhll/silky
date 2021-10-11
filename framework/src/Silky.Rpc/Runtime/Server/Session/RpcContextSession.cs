using System;
using System.Security.Claims;
using Silky.Core.Rpc;
using Silky.Rpc.Security;

namespace Silky.Rpc.Runtime.Server
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
                var userId = RpcContext.Context.GetAttachment(ClaimTypes.NameIdentifier);
                return userId;
            }
        }

        public override string UserName
        {
            get
            {
                var userName = RpcContext.Context.GetAttachment(ClaimTypes.Name);
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
                var tenantId = RpcContext.Context.GetAttachment(SilkyClaimTypes.TenantId);
                return tenantId;
            }
        }
    }
}