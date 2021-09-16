using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Handlers
{
    internal interface IMessageReceivedHandler
    {
        Task Handle([NotNull] ServiceEntry serviceEntry);
    }
}