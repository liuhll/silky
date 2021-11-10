---
title: 主机
lang: zh-cn
---

## 主机的概念

silky的主机与.net的[主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1)概念一致。是封装应用资源的对象,用于托管应用和管理应用的生命周期。

### 通用主机

如果用于托管普通的业务应用,该微服务模块本身并不需要对直接对集群外部提供访问入口。那么,您可以使用[.net的通用主机](https://docs.microsoft.com/zh-cn/dotnet/core/extensions/generic-host)注册silky服务框架。.net的通用主机无法提供http请求,也无法配置http的请求管道(即:**中间件**)。

在注册silky框架后,silky框架会注册dotnetty的服务监听者,并会暴露rpc端口号。但是由于silky框架的安全机制,集群外部并不允许通过`tcp`协议通过rpc端口号直接访问该微服务模块的应用接口。

### web主机

如果您需要访问该服务模块的应用接口,您必须要通过.net的[web主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-3.1)注册silky框架,并配置silky框架的请求管道。这样,web构建的主机通过引用某个微服务的应用接口项目(包)，通过应用接口的代理与微服务集群内部实现rpc通信。

## 业务主机类型

silky微服务框架提供了多种类型的业务主机,开发者可以选择合适的主机来托管应用服务。

### 使用web主机构建微服务应用

使用web主机构建的silky应用具有如下特性:

1. 提供http服务和RPC服务,暴露http端口和RPC端口
2. 可以作为外部流量的入口,集群外部通过http服务访问微服务应用集群
3. 作为RPC服务提供者,通过RPC框架与其他微服务进行通信

一般地,如果我们希望该服务应用既可以作为RPC服务提供者,也希望外部能够直接通过http协议访问应用,那么我们就可以通过web主机构建微服务应用。这样的方式适用于将微服务应用拆分给不同的团队进行开发,开发者也无需要额外的构建网关,就可以访问微服务应用服务。

使用web主机构建Silky微服务应用只需要开发者安装`Silky.Agent.Host`包后,在`Main()`方法中通过`Host`提供的API`ConfigureSilkyWebHostDefaults`即可。开发者需要指定`Startup`类,在`Startup`中注册服务和配置http中间件。

```csharp
namespace Silky.Sample
{
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .ConfigureSilkyWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
               
        }
    }
}
```

当然,我们也可以在构建主机的时候,另外指定启动模块:

```csharp
namespace Silky.Sample
{
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .ConfigureSilkyWebHost<DemoModule>(webBuilder => webBuilder.UseStartup<Startup>());
               
        }
    }
}
```

自定义的启动模块`DemoModule`需要继承`WebHostModule`,开发者可以在自定义的启动模块中,定义应用启动和停止需要执行的业务方法和配置服务注册,也可以通过依赖开发者扩展的自定义模块。

```csharp
    // 依赖开发者自定义的模块
    // [DependsOn(typeof("UserDefinedModule"))]
    public class DemoModule : WebHostModule
    {
        public override Task Initialize(ApplicationContext applicationContext)
        {
            // 开发者可以定义应用程序启动时执行的业务方法
            return base.Initialize(applicationContext);
        }

        public override Task Shutdown(ApplicationContext applicationContext)
        {
            // 开发者可以定义应用程序停止时执行的业务方法
            return base.Shutdown(applicationContext);
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // 开发者可以配置服务注册,作用与Startup类ConfigureServices一致
            base.ConfigureServices(services, configuration);
        }
    }
```

在启动类`Startup`类中配置服务注册和中间件:

```csharp
 public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) 
        {
            services
                .AddSilkyHttpCore()
                .AddResponseCaching()
                .AddHttpContextAccessor()
                .AddRouting()
                .AddSilkyIdentity()
                .AddSilkyMiniProfiler()
                .AddSwaggerDocuments();
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocuments();
                app.UseMiniProfiler();
            }
            app.UseRouting();
            app.UseResponseCaching();
            app.UseSilkyWebSocketsProxy();
            app.UseSilkyIdentity();
            app.UseSilkyHttpServer();           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSilkyRpcServices();
            });
        }
    }
```

这样,我们就可以得到一个既可以提供http服务,也作为rpc服务提供者的应用。

### 使用通用主机构建微服务应用

使用通用主机构建的silky应用具有如下特性:

1. 只提供RPC服务,不提供http服务,微服务集群外部无法直接访问应用
2. 可以通过网关或是具有http服务的应用间接的访问该微服务提供的服务

![host1.png](/assets/imgs/host1.png)

一般地,如果只是作为普通的业务应用,只需要作为RPC服务提供者,服务内部通过RPC框架进行通信,并不需要对外提供http服务,在这样的情况下,我们考虑使用通用主机构建微服务应用。

开发者在安装`Silky.Agent.Host`包后,在`Main()`方法中通过`Host`提供的API`ConfigureSilkyGeneralHostDefaults`即可通过通用主机构建silky微服务应用。

```csharp
namespace Silky.Sample
{
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyGeneralHostDefaults();
    }
}
```

同样地,我们也可以在构建主机的时候,另外指定启动模块:

```csharp
namespace Silky.Sample
{
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSilkyGeneralHost<DemoModule>();
    }
}
```

在这里,我们需要自定义的启动模块`DemoModule`需要继承`GatewayHostModule`,开发者可以在自定义的启动模块中,定义应用启动和停止需要执行的业务方法和配置服务注册,也可以通过依赖开发者扩展的自定义模块。

```csharp
  // [DependsOn(typeof("UserDefinedModule"))]
    public class DemoModule : GatewayHostModule
    {
        public override Task Initialize(ApplicationContext applicationContext)
        {
            // 开发者可以定义应用程序启动时执行的业务方法
            return base.Initialize(applicationContext);
        }

        public override Task Shutdown(ApplicationContext applicationContext)
        {
            // 开发者可以定义应用程序停止时执行的业务方法
            return base.Shutdown(applicationContext);
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // 开发者可以配置服务注册,作用与Startup类ConfigureServices一致
            base.ConfigureServices(services, configuration);
        }
    }
```

::: warning 注意

与web主机构建微服务应用自定义启动模块继承的基类不同,但是作用和使用上一致

:::

通用主机构建的微服务应用,不提供HTTP服务,所有也无需(也没有必要)配置http中间件。但是,开发者可以通过继承`IConfigureService`来配置服务的注册,从而自身服务注册,或是引入第三方组件。

```csharp
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkyCaching()
                .AddSilkySkyApm()
                .AddMessagePackCodec();
            
            // 可以通过服务注册引入第三方组件,例如:CAP,MassTransit等
            
            services.AddDatabaseAccessor(
                options => { options.AddDbPool<DefaultContext>(); },
                "Demo.Database.Migrations");
        }
    }
```

### 构建具有websocket能力的微服务应用

### 构建网关
