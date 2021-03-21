using System.Threading.Tasks;
using Lms.Rpc.Routing;
using Microsoft.AspNetCore.Http;

namespace Lms.HttpServer.Handlers
{
    public interface IWsShakeHandHandler
    {
        Task Connection(ServiceRoute serviceRoute, HttpContext httpContext);
    }
}