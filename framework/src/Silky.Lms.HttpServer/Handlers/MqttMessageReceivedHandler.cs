using System.Threading.Tasks;
using Silky.Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;

namespace Silky.Lms.HttpServer.Handlers
{
    public class MqttMessageReceivedHandler : IMessageReceivedHandler
    {
        public Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            throw new System.NotImplementedException();
        }
    }
}