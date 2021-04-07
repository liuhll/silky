using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.AutoMapper
{
    public interface IAutoMapperBootstrap : ITransientDependency
    {
        void Initialize();
    }
}