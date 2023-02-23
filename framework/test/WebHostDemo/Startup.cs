using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebHostDemo;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // 新增必要的服务
        services.AddSilkyHttpCore()
            .AddSwaggerDocuments()
            .AddRouting();
        services.AddSilkyIdentity();
        services.AddDashboard();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 判断是否开发环境
        if (env.IsDevelopment())
        {
            // 开发环境使用Swagger在线文档
            app.UseSwaggerDocuments();
        }

        // 使用路由中间件
        app.UseRouting();

        // 添加其他asp.net core中间件...
        app.UseDashboard();
        app.UseSilkyIdentity();
        app.UseSilkyWrapperResponse();
        // 配置路由
        app.UseEndpoints(endpoints =>
        {
            // 配置SilkyRpc路由
            endpoints.MapSilkyRpcServices();
        });
    }
}