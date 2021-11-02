---
title: 快速开始
lang: zh-cn
---

## 必要前提

1. (**必须**) 安装 .net5 或是 .net6 sdk。

2. (**必须**) 您可以使用visual studio 或是rider作为开发工具。 

3. (**必须**) 您必须准备一个可用的`zookeeper`服务作为服务注册中心。

4. (**可选**) 你可以选择`redis`服务作为分布式缓存服务。


## 使用web host构建微服务应用 

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

3. 在`Main`方法中构建构建silky主机

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

4. 在启用类中配置服务和配置中间件和路由

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

silky支持通过`json`或是`yml`格式进行配置。您可以通过`appsettings.json`为公共配置项指定配置信息,也可以通过新增`appsettings.${ENVIRONMENT}.json`文件为指定的环境配置信息。

一般地,您必须要指定rpc通信的`token`,服务注册中心地址,数据库连接等配置项。如果您使用redis作为缓存服务,那么您还需要将`distributedCache:redis:isEnabled`配置项设置为`true`,并给出redis服务缓存的地址。

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

将[docker-compose.zookeeper.yml](https://raw.githubusercontent.com/liuhll/silky/main/samples/docker-compose/infrastr/docker-compose.zookeeper.yml)和[docker-compose.redis.yml](https://raw.githubusercontent.com/liuhll/silky/main/samples/docker-compose/infrastr/docker-compose.redis.yml)拷贝到本地,保存为相同名称文件,进入到保存文件的本地目录。

```powershell
# 创建一个名称为silky_service_net的docker网络
docker network create silky_service_net

# 使用docker-compose创建zookeeper和redis服务
docker-compose -f docker-compose.zookeeper.yml -f docker-compose.redis.yml up -d
```


6. 微服务应用的其他层(项目)

完成主机项目构建后,您可以新增**应用接口层**、**应用层**、**领域层**、**基础设施层**等其他项目,更多内容请参考[微服务架构](#)节点。

一个典型的微服务模块的划分与传统的`DDD`领域模型的应用划分基本一致。需要将应用接口单独的抽象为一个程序集，方便被其他微服务应用引用，其他微服务应用通过应用接口生成RPC代理,与该微服务通信。

一个典型的微服务模块的项目结构如下所示:

![quick-start4.png](/assets/imgs/quick-start4.png)

项目的依赖关系如下:

1) 主机项目依赖**应用层**,从而达到对应用的托管。
2) **应用接口层**用于定义服务接口和`DTO`对象,**应用层**需要依赖**应用接口层**,实现定义好的服务接口。
3) **领域层**主要用于实现具体的业务逻辑,可以依赖自身的**应用接口层**以及其他微服务应用的**应用接口层**(开发者可以通过nuget包安装其他微服务应用的应用接口项目或是直接添加项目的方式进行引用);**领域层**依赖自身的**应用接口层**的原因是为了方便使用`DTO`对象;引用其他微服务的**应用接口层**可以通过接口生成的动态代理,与其他微服务通过`SilkyRPC`框架进行通信。
4) **领域共享层(Domain.Shared)**一般用于定义ddd概念中的值类型以及枚举等,可以被其他微服务应用引用。
5) **EntityFramework**作为基础服务层,提供数据访问能力,当然,开发者也可以选择使用其他ORM框架。


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