using System.Threading.Tasks;
using JetBrains.Annotations;
using Silky.Rpc.Runtime.Server;
using Microsoft.AspNetCore.Http;

namespace Silky.Http.Core.Handlers
{
    internal interface IMessageReceivedHandler
    {
        Task Handle([NotNull] HttpContext context, [NotNull] ServiceEntry serviceEntry);
    }
}