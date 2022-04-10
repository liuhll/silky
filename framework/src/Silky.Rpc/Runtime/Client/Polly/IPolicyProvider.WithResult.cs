using Polly;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client
{
    public interface IPolicyWithResultProvider : ISingletonDependency
    {
        IAsyncPolicy<object> Create(string serviceEntryId);
    }
}