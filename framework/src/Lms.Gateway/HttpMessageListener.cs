using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Messages;
using Lms.Rpc.Runtime.Server.ServiceEntry;
using Lms.Rpc.Transport;

namespace Lms.Gateway
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