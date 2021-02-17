using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Security
{
    public interface ITokenValidator : ITransientDependency
    {
        bool Validate();
    }
}