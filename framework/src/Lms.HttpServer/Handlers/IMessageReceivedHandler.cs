using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;

namespace Lms.HttpServer.Handlers
{
    internal interface IMessageReceivedHandler
    {
        Task Handle([NotNull] HttpContext context, [NotNull] ServiceEntry serviceEntry);
    }
}