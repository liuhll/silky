using System.Threading.Tasks;
using Lms.DotNetty.Protocol.Ws;

namespace IAnotherApplication
{
    [WsServiceRoute(2120)]
    public interface IWsTestAppService
    {
        Task Echo(string msg);
    }
}