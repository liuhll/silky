using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using IAnotherApplication;
using Lms.DotNetty.Protocol.Ws;

namespace AnotherHostDemo.AppService
{
    public class WsTestAppService : WsAppServiceBase, IWsTestAppService
    {
        public Task Echo(string msg)
        {
            throw new System.NotImplementedException();
        }

        protected override Task OnOpen(IChannelHandlerContext context)
        {
            throw new System.NotImplementedException();
        }

        protected override Task OnClose(IChannelHandlerContext context)
        {
            throw new System.NotImplementedException();
        }

        protected override Task OnReceive(IChannelHandlerContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}