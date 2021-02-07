using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Messages;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport;

namespace Lms.HttpServer
{
    public class HttpMessageListener : IMessageListener, ISingletonDependency
    {
        private readonly IServiceEntryLocate _serviceEntryLocate;

        public HttpMessageListener(IServiceEntryLocate serviceEntryLocate)
        {
            _serviceEntryLocate = serviceEntryLocate;
        }


        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}