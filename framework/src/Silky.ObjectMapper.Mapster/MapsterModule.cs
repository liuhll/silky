using System.Linq;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core;
using Silky.Core.Modularity;

namespace Silky.ObjectMapper.Mapster
{
    public class MapsterModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // 获取全局映射配置
            var config = TypeAdapterConfig.GlobalSettings;
            var assemblies = EngineContext.Current.TypeFinder.FindClassesOfType<IRegister>().Select(p => p.Assembly)
                .Distinct().ToArray();
            // 扫描所有继承  IRegister 接口的对象映射配置
            if (assemblies.Any()) config.Scan(assemblies);

            // 配置默认全局映射（支持覆盖）
            config.Default
                .NameMatchingStrategy(NameMatchingStrategy.Flexible)
                .PreserveReference(true);

            // 配置支持依赖注入
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
            
        }
    }
}