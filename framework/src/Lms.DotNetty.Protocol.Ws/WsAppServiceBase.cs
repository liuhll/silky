using System.Threading.Tasks;

namespace Lms.DotNetty.Protocol.Ws
{
    public abstract class WsAppServiceBase
    {
       public abstract Task OnOpen();

       public abstract Task OnClose();

       public abstract Task OnReceive();
    }
}