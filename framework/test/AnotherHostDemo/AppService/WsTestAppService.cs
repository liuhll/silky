using System.Threading.Tasks;
using IAnotherApplication;
using Lms.WebSocket;
using WebSocketSharp;

namespace AnotherHostDemo.AppService
{
    public class WsTestAppService : WsAppServiceBase, IWsTestAppService
    {
        protected override void OnOpen()
        {
            base.OnOpen();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
        }

        public async Task Echo(string msg)
        {
            Send(msg);
        }
    }
}