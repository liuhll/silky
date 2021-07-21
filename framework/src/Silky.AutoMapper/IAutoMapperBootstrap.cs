using Silky.Core.DependencyInjection;

namespace Silky.AutoMapper
{
    public interface IAutoMapperBootstrap : ITransientDependency
    {
        void Initialize();
    }
}