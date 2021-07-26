using Silky.Core.DependencyInjection;

namespace Silky.ObjectMapper.AutoMapper
{
    public interface IAutoMapperBootstrap : ITransientDependency
    {
        void Initialize();
    }
}