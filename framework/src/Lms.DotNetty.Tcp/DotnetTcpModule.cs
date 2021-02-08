using System.Threading.Tasks;
using Autofac;
using Lms.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.DotNetty.Tcp
{
    public class DotnetTcpModule : LmsModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DotNettyTcpServerMessageListener>()
                .AsSelf()
                .PropertiesAutowired()
                .AsImplementedInterfaces();
        }

        public async override Task Initialize(ApplicationContext applicationContext)
        {
            var messageListener = applicationContext.ServiceProvider.GetService<DotNettyTcpServerMessageListener>();
            await messageListener.Listen();
        }
    }
}