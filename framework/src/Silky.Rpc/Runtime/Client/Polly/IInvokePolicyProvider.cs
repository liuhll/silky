using Polly;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Client
{
    public interface IInvokePolicyProvider : ISingletonDependency
    {
        IAsyncPolicy Create(string serviceEntryId);
    }
}