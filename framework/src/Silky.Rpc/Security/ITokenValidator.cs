using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Security
{
    public interface ITokenValidator : ITransientDependency
    {
        bool Validate();
    }
}