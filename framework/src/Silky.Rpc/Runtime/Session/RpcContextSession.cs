using System;
using Silky.Core.Rpc;
using Silky.Rpc.Security;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Session
{
    public class RpcContextSession : SessionBase
    {
        internal RpcContextSession()
        {
        }

        public override long? UserId
        {
            get
            {
                var userId = RpcContext.Context.GetAttachment(ClaimTypes.UserId);
                if (userId != null)
                {
                    return Convert.ToInt64(userId);
                }

                return null;
            }
        }

        public override string UserName
        {
            get
            {
                var userName = RpcContext.Context.GetAttachment(ClaimTypes.UserName);
                if (userName != null)
                {
                    return userName.ToString();
                }

                return null;
            }
        }
        
        public override long? TenantId
        {
            get
            {
                var tenantId = RpcContext.Context.GetAttachment(ClaimTypes.TenantId);
                if (tenantId != null)
                {
                    return Convert.ToInt64(tenantId);
                }

                return null;
            }
        }
    }
}