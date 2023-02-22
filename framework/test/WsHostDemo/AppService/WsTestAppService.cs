using System.Threading.Tasks;
using IAnotherApplication;
using Silky.Core.Exceptions;
using Silky.WebSocket;
using WebSocketSharp;

namespace WsHostDemo.AppService
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
        

        protected override void OnMessage(MessageEventArgs e)
        {
            // foreach (var businessSessionInfo in BusinessSessionIds)
            // {
            //     foreach (var businessSessionId in businessSessionInfo.Value)
            //     {
            //         if (SessionManager.TryGetSession(businessSessionId, out var session))
            //         {
            //             SessionManager.SendTo($"message:{e.Data},sessionId:{session.ID},BusinessIds:{businessSessionInfo.Key}", businessSessionId);
            //         }
            //     }
            // }
            SessionManager.SendTo($"message:{e.Data},sessionId:{ID}", ID);
        }
    }
}