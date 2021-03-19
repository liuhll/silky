using System.Threading.Tasks;
using IAnotherApplication;
using Lms.DotNetty.Protocol.Ws;

namespace AnotherHostDemo.AppService
{
    public class WsTestAppService : WsAppServiceBase, IWsTestAppService
    {
        public override Task OnOpen()
        {
            throw new System.NotImplementedException();
        }

        public override Task OnClose()
        {
            throw new System.NotImplementedException();
        }

        public override Task OnReceive()
        {
            throw new System.NotImplementedException();
        }

        public Task Echo(string msg)
        {
            throw new System.NotImplementedException();
        }
    }
}