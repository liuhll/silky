using Autofac;
using Lms.Core.Modularity;

namespace ConsoleDemo
{
    public class ConsoleDemoModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            base.RegisterServices(builder);
        }
    }
}