using Lms.Core.DependencyInjection;

namespace Lms.AutoMapper
{
    public interface IAutoMapperBootstrap : ITransientDependency
    {
        void Initialize();
    }
}