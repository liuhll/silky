using System.Threading.Tasks;
using IAnotherApplication;
using Silky.Lms.WebSocket;

namespace AnotherHostDemo.AppService
{
    public class WsTestAppService : WsAppServiceBase, IWsTestAppService
    {
        public async Task Echo(string msg)
        {
            foreach (var session in SessionManager.Sessions)
            {
                SessionManager.SendTo(msg, session.ID);
            }
        }
    }
}