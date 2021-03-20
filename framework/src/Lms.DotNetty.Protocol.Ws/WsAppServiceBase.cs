using System.Threading.Tasks;
using DotNetty.Transport.Channels;

namespace Lms.DotNetty.Protocol.Ws
{
    public abstract class WsAppServiceBase
    {
        protected abstract Task OnOpen(IChannelHandlerContext context);

        protected abstract Task OnClose(IChannelHandlerContext context);

        protected abstract Task OnReceive(IChannelHandlerContext context);
    }
}