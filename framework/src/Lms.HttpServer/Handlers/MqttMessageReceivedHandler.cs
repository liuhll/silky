using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;

namespace Lms.HttpServer.Handlers
{
    public class MqttMessageReceivedHandler : IMessageReceivedHandler
    {
        public Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            throw new System.NotImplementedException();
        }
    }
}