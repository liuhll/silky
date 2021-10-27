using Polly;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client
{
    public interface IPolicyWithResultProvider : IScopedDependency
    {
        IAsyncPolicy<object> Create(string serviceEntryId, object[] parameters);
    }
}