using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Rpc.Security
{
    public interface ITokenValidator : ITransientDependency
    {
        bool Validate();
    }
}