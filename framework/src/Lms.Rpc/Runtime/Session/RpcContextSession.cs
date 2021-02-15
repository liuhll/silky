using System;
using Lms.Rpc.Security;
using Lms.Rpc.Transport;

namespace Lms.Rpc.Runtime.Session
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
                var userId = RpcContext.GetContext().GetAttachment(ClaimTypes.UserId);
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
                var userName = RpcContext.GetContext().GetAttachment(ClaimTypes.UserName);
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
                var tenantId = RpcContext.GetContext().GetAttachment(ClaimTypes.TenantId);
                if (tenantId != null)
                {
                    return Convert.ToInt64(tenantId);
                }

                return null;
            }
        }
    }
}