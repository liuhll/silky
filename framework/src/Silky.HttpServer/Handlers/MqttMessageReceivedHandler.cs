using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;

namespace Silky.HttpServer.Handlers
{
    public class MqttMessageReceivedHandler : IMessageReceivedHandler
    {
        public Task Handle(HttpContext context, ServiceEntry serviceEntry)
        {
            throw new System.NotImplementedException();
        }
    }
}