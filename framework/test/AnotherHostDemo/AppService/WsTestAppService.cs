using System.Threading.Tasks;
using IAnotherApplication;
using Silky.Core.Exceptions;
using Silky.WebSocket;

namespace AnotherHostDemo.AppService
{
    public class WsTestAppService : WsAppServiceBase, IWsTestAppService
    {
        public async Task Echo(string businessId, string msg)
        {
            if (BusinessSessionIds.TryGetValue(businessId, out var sessionIds))
            {
                foreach (var sessionId in sessionIds)
                {
                    SessionManager.SendTo($"message:{msg},sessionId:{sessionId}", sessionId);
                }
            }
            else
            {
                throw new BusinessException($"不存在businessId为{businessId}的会话");
            }
        }
    }
}