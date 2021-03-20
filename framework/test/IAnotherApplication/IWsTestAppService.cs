using System.Threading.Tasks;
using Lms.DotNetty.Protocol.Ws;

namespace IAnotherApplication
{
    [WsServiceRoute(1010)]
    public interface IWsTestAppService
    {
        Task Echo(string msg);
    }
}