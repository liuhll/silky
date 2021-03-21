using System.Threading.Tasks;
using Lms.DotNetty.Protocol.Ws;

namespace IAnotherApplication
{
    [WsServiceRoute()]
    public interface IWsTestAppService
    {
        Task Echo(string msg);
    }
}