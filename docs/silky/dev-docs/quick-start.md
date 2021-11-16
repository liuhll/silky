---
title: 快速开始
lang: zh-cn
---

## 必要前提

1. (**必须**) 安装 .net5 或是 .net6 sdk。

2. (**必须**) 您可以使用visual studio 或是rider作为开发工具。 

3. (**必须**) 您必须准备一个可用的`zookeeper`服务作为服务注册中心。

4. (**必须**) 使用选择`redis`服务作为分布式缓存服务。


## 使用Web主机构建微服务应用 

开发者可以通过.net平台提供[Web 主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0)来构建silky微服务应用。

使用webhost来构建的Silky微服务应用，不但可以作为微服务应用的服务提供者(服务内部可以通过SilkyRpc框架进行通信);也提供http服务，http请求通过应用服务方法(服务条目)生成的webapi,通过silky设定的路由规则即可访问微服务应用提供的相关服务。

我们通过如下步骤可以快速的构建一个使用Web 主机构建的Silky微服务应用。

1. 新增一个控制台应用或是ASP.NET Core Empty应用

![quick-start1.png](/assets/imgs/quick-start1.png)

![quick-start1.1.png](/assets/imgs/quick-start1.1.png)

2. 安装`Silky.Agent.Host`包

通过 Nuget Package Manger 安装`Silky.Agent.Host`包:

![quick-start2.png](/assets/imgs/quick-start2.png)

或是通过控制台命令安装包:

```powershell
PM> Install-Package Silky.Agent.Host -Version 3.0.2
```

3. 在`Main`方法中构建silky主机

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

4. 在启用类中配置服务和置中间件、路由

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silky.Http.Core;

namespace Silky.Sample
{
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 判断是否开发环境
            if (env.IsDevelopment())
            {
                // 开发环境使用开发者异常调式页面
                app.UseDeveloperExceptionPage();
                // 开发环境使用Swagger在线文档
                app.UseSwaggerDocuments();
            }

            // 使用路由中间件
            app.UseRouting();
            
            // 添加其他asp.net core中间件...

            // 配置路由
            app.UseEndpoints(endpoints => 
              { 
                // 配置SilkyRpc路由
                endpoints.MapSilkyRpcServices(); 
              });
        }
    }
}
```

4. 更新配置

silky支持通过`json`或是`yml`格式进行配置。您可以通过`appsettings.json`为公共配置项指定配置信息,也可以通过新增`appsettings.${ENVIRONMENT}.json`文件为指定的环境更新配置属性。

一般地,您必须指定rpc通信的`token`,服务注册中心地址等配置项。如果您使用redis作为缓存服务,那么您还需要将`distributedCache:redis:isEnabled`配置项设置为`true`,并给出redis服务缓存的地址。

在`appsettings.json`配置文件中新增如下配置属性:

```json
{
  "RegistryCenter": {
    "Type": "Zookeeper",
    "ConnectionStrings": "127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186"
  },
  "DistributedCache": {
    "Redis": {
      "IsEnabled": true,
      "Configuration": "127.0.0.1:6379,defaultDatabase=0"
    }
  },
  "Rpc": {
    "Token": "ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW",
    "Port": 2200
  }
}
```

将配置文件属性的**复制到输出目录**,设置为: *始终复制* 或是 *如果较新则复制*。

![quick-start3.png](/assets/imgs/quick-start3.png)

5. 创建zookeeper服务和redis缓存服务

在该示例项目中,我们使用`Zookeeper`作为服务注册中心。我们在silky的示例项目中给出各种基础服务的[docker-compose的编排文件](https://github.com/liuhll/silky/tree/main/samples/docker-compose/infrastr),其中,也包括了zookeeper和redis服务的。

将[docker-compose.zookeeper.yml](https://raw.githubusercontent.com/liuhll/silky/main/samples/docker-compose/infrastr/docker-compose.zookeeper.yml)和[docker-compose.redis.yml](https://raw.githubusercontent.com/liuhll/silky/main/samples/docker-compose/infrastr/docker-compose.redis.yml)拷贝到本地,保存为相同名称的文件,进入到保存文件的本地目录。

```powershell
# 创建一个名称为silky_service_net的docker网络
docker network create silky_service_net

# 使用docker-compose创建zookeeper和redis服务
docker-compose -f docker-compose.zookeeper.yml -f docker-compose.redis.yml up -d
```


6. 微服务应用的其他层(项目)

完成主机项目后,您可以新增**应用接口层**、**应用层**、**领域层**、**基础设施层**等其他项目,更多内容请参考[微服务架构](#)节点。

一个典型的微服务模块的划分与传统的`DDD`领域模型的应用划分基本一致。需要将应用接口单独的抽象为一个程序集，方便被其他微服务应用引用，其他微服务应用通过应用接口生成RPC代理,与该微服务通信。

一个典型的微服务模块的项目结构如下所示:

![quick-start4.png](/assets/imgs/quick-start4.png)

项目的依赖关系如下:

(1) 主机项目依赖**应用层**,从而达到对应用的托管。

(2) **应用接口层**用于定义服务接口和`DTO`对象,**应用层**需要依赖**应用接口层**,实现定义好的服务接口。

(3) **领域层**主要用于实现具体的业务逻辑,可以依赖自身的**应用接口层**以及其他微服务应用的**应用接口层**(开发者可以通过nuget包安装其他微服务应用的应用接口项目或是直接添加项目的方式进行引用);**领域层**依赖自身的**应用接口层**的原因是为了方便使用`DTO`对象;引用其他微服务的**应用接口层**可以通过接口生成的动态代理,与其他微服务通过`SilkyRPC`框架进行通信。

(4) **领域共享层(Domain.Shared)** 一般用于定义ddd概念中的值类型以及枚举等,方便被其他微服务应用引用。

(5) **EntityFramework**作为基础服务层,提供数据访问能力,当然,开发者也可以选择使用其他ORM框架。


7. 应用接口的定义和实现

**应用接口层(Silky.Sample.Application.Contracts)** 安装包`Silky.Rpc`:

![quick-start5.png](/assets/imgs/quick-start5.png)

或是通过控制台命令安装包:

```powershell
PM> Install-Package Silky.Rpc -Version 3.0.2
```

新增一个服务接口`IGreetingAppService`,并且定义一个`Say()`方法,应用接口需要使用`[ServiceRoute]`特性进行标识。

```csharp
[ServiceRoute]
public interface IGreetingAppService
{
    Task<string> Say(string line);
}
```

接下来,我们需要 **应用层(Silky.Sample.Application)** 依赖(引用) **应用接口层(Silky.Sample.Application.Contracts)**, 并新增一个服务类`GreetingAppService`,通过它实现服务接口`IGreetingAppService`。

```csharp
    public class GreetingAppService : IGreetingAppService
    {
        public Task<string> Say(string line)
        {
            return Task.FromResult($"Hello {line}");
        }
    }
```

8. 通过Swagger文档在线调试

运行应用程序,即可打开swagger在线文档。开发者可以通过swagger生成的在线文档调试API。

![quick-start6.png](/assets/imgs/quick-start6.png)

## 使用.NET通用主机构建微服务应用

开发者可以通过.net平台提供[通用主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-6.0)来构建silky微服务应用。

使用.NET 通用主机构建微服务应用只能作为服务提供者,通过SilkyRPC框架与其他微服务应用进行通信;无法提供http服务，也就是说,集群外部无法直接访问该微服务应用，只能通过网关或是其他提供http服务的微服务应用访问该微服务应用的服务。

使用.NET 通用主机构建Silky微服务应用的步骤与使用使用Web 主机构建微服务应用的步骤基本一致,区别在于无需配置`Startup`类,也不能配置http中间件(配置了也无效);开发者可以通过实现`IConfigureService`接口来完成对服务注入的配置。

1-2 步骤与[使用web主机构建微服务应用](#使用Web主机构建微服务应用)一致。

3. 在`Main`方法中构建silky主机

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
                    .ConfigureSilkyGeneralHostDefaults();
               
        }
    }
}
```
创建`ConfigureService`类,用于实现`IConfigureService`接口,在`ConfigureServices()`方法中配置服务注入依赖。

```csharp
   public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm()
                //其他服务(包括第三方组件的服务或是silky框架的其他服务,例如:Efcore组件,MessagePack编解码,Cap或是MassTransit等分布式事件总线等)
                //...
                ;
        }
    }
```

5-7步骤与[使用web主机构建微服务应用](#使用Web主机构建微服务应用)一致。

启动应用后,我们可以在控制台看到相关的日志输出,应用服务启动成功。

![quick-start7.png](/assets/imgs/quick-start7.png)

用户无法直接访问该微服务应用,必须通过网关引用该微服务的 **应用接口层** ,通过[网关](#构建Silky微服务网关)的提供的http服务间接的访问该微服务应用提供的服务。


## 构建具有websocket服务能力的微服务应用

开发者通过构建具有websocket服务能力的微服务应用, 这样的微服务应用可以除了可以作为服务提供者之外,还具有提供websocket通信的能力(websocket端口默认为:3000)。可以通过与服务端进行握手会话(可以通过网关代理),服务端实现向客户单推送消息的能力。


构建具有websocket服务能力的微服务应用与[使用.NET通用主机构建微服务应用](#使用.NET通用主机构建微服务应用)的步骤一致,只是用于构建微服务应用的方法有差异。

1-2 步骤与[使用web主机构建微服务应用](#使用Web主机构建微服务应用)一致。

3. 在`Main`方法中构建silky主机

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
                .ConfigureSilkyWebSocketDefaults();
    }
}

```

创建`ConfigureService`类,用于实现`IConfigureService`接口,在`ConfigureServices()`方法中配置服务注入依赖。

```csharp
   public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm()
                //其他服务(包括第三方组件的服务或是silky框架的其他服务,例如:Efcore组件,MessagePack编解码,Cap或是MassTransit等分布式事件总线等)
                //...
                ;
        }
    }
```

5-6步骤与[使用web主机构建微服务应用](#使用Web主机构建微服务应用)一致。

7. 构建具有提供websocket服务能力的服务

应用服务接口的定义与一般应用服务的接口定义一样,只需要在一个普通的接口标识`[ServiceRoute]`特性即可。

```csharp

[ServiceRoute]
public interface ITestAppService
{
   // 可以定义其他方法(服务条目),定义的方法可以与其他微服务应用通过RPC框架进行通信
}
```


我们需要在 **应用层(Silky.Sample.Application)** 安装 `Silky.WebSocket`包。

```powershell
PM> Install-Package Silky.WebSocket -Version 3.0.2
```

并新增一个 `TestAppService`类, 通过它来实现 `ITestAppService`, 除此之外,我们需要 `TestAppService`类继承 `WsAppServiceBase`基类。

```csharp
    public class TestAppService : WsAppServiceBase, ITestAppService
    {
        private readonly ILogger<TestAppService> _logger;

        public TestAppService(ILogger<TestAppService> logger)
        {
            _logger = logger;
        }

        // 当建立websocket会话时
        protected override void OnOpen()
        {
            base.OnOpen();
            _logger.LogInformation("websocket established a session");
            
        }

        // 当服务端接收到客服端的消息时
        protected override void OnMessage(MessageEventArgs e)
        {
            _logger.LogInformation(e.Data);
        }
        
       // 当websocket会话关闭时
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            _logger.LogInformation("websocket disconnected");
        }

        // 其他服务方法
    }
```

启动应用后,我们可以在控制台看到相关的日志输出,应用服务启动成功。我们定义的websocket的服务的webapi地址为:`/api/test`。

![quick-start8.png](/assets/imgs/quick-start8.png)

8. 客户端透过网关代理与websocket服务握手

客户端无法直接与该微服务应用进行握手,必须通过网关引用该微服务的 **应用接口层** ,通过[网关](#构建Silky微服务网关)的提供的websocket代理服务与该微服务进行握手,通过`ws[s]://gateway_ip[:gateway_port]/websocket_webapi`与之前定义websocket服务进行会话。

我们在构建的网关应用中引用该微服务的**应用接口层**,并启动网关应用(网关服务地址为`127.0.0.1:5000`),并可通过地址:`ws://127.0.0.1:5000/api/test`与之前定义的websocket服务进行握手和通信。

客户端与websocket服务进行握手时,需要通过`qstring参数`或是请求头设置`hashkey`，确保每次通信的微服务应用都是同一个实例。

![quick-start9.png](/assets/imgs/quick-start9.png)

![quick-start10.png](/assets/imgs/quick-start10.png)

## 构建Silky微服务网关

实际上,[通过.net平台提供Web主机](#使用Web主机构建微服务应用)来构建silky微服务应用，也可以认为是一个网关。我们在这里专门构建的网关与[通过.net平台提供Web 主机](#使用Web主机构建微服务应用)的区别在于该类型的微服务应用只能作为服务消费者,不能作为RPC服务提供者。

总的来说,网关是对微服务应用集群来说是一个对接外部的流量入口。

构建过程与[通过.net平台提供Web 主机](#使用Web主机构建微服务应用)一致,我们只需要将创建主机的方法修改为:

```csharp
 private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureSilkyGatewayDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
```

网关项目通过引用其他微服务应用的**应用接口层**，就可以作为服务消费者通过SilkyRPC框架调用其他微服务应用提供的服务,并且通过网关提供的http相关中间件可以实现生成在线swagger文档,实现统一的api鉴权,http限流,生成dashboard管理端,实现对微服务集群服务提供者实例的健康检查等功能。