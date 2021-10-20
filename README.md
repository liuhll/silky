<p align="center">
  <img height="200" src="./docs/.vuepress/public/assets/logo/logo.svg">
</p>

# Silky 微服务框架
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![Commit](https://img.shields.io/github/last-commit/liuhll/silky)](https://img.shields.io/github/last-commit/liuhll/silky)
[![NuGet](https://img.shields.io/nuget/v/silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![MyGet (nightly builds)](https://img.shields.io/myget/silky-preview/vpre/Silky.Core.svg?style=flat-square)](https://www.myget.org/feed/Packages/silky-preview)
[![NuGet Download](https://img.shields.io/nuget/dt/Silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2Fliuhll%2Fsilky&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)

<div align="center">

**简体中文 | [English](./README.en-US.md)**

</div>

## 项目介绍

Silky框架旨在帮助开发者在.net平台下,通过简单代码和配置快速构建一个微服务开发框架。

Silky 通过 .net core的[主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/?view=aspnetcore-5.0&tabs=macos#host)来托管微服务应用。通过 Asp.Net Core 提供的http服务接受外界请求，转发到后端的微服务应用，服务内部通过[DotNetty](https://github.com/Azure/DotNetty)实现的`SilkyRpc`框架进行通信。

## 入门

- 通过[开发者文档](http://docs.silky-fk.com/silky/)学习Silky框架。
- 通过[silky.samples项目](http://docs.silky-fk.com/silky/dev-docs/quick-start.html)熟悉如何使用Silky框架构建一个微服务应用。
- 通过[配置](http://docs.silky-fk.com/config/)文档熟悉Silky框架的相关配置属性。

## 框架特性

- 面向接口代理的高性能RPC调用
- 服务自动注册和发现,支持Zookeeper、Consul、Nacos作为服务注册中心
- 智能容错和负载均衡，强大的服务治理能力
- 支持缓冲拦截
- 高度可扩展能力
- 支持分布式事务
- 流量监控
- 通过SkyApm进行链路跟踪
- 通过Swagger生成在线API文档


## 快速开始

### 1. 构建主机

新建一个web或是控制台项目,通过 nuget安装`Silky.Agent.Host`包。

```pwsh
PM> Install-Package Silky.Agent.Host
```

在`Main`方法中通过`HostBuilder`构建主机。

```csharp
public class Program
{
  public static Task Main(string[] args)
  {
    return CreateHostBuilder(args).Build().RunAsync();
  }

  private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
      .ConfigureSilkyWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>();});
   
}
```

在`Startup`中配置服务依赖注入，以及配置中间件。

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSilkyHttpCore()
        .AddSwaggerDocuments()
        .AddRouting();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  if (env.IsDevelopment())
  {
    app.UseDeveloperExceptionPage();
    app.UseSwaggerDocuments();
  }

  app.UseRouting();

  app.UseEndpoints(endpoints => { endpoints.MapSilkyRpcServices(); });
}
```

### 2. 更新配置

在配置文件中指定服务注册中心的类型和服务注册中心配置属性以及`SilkyRpc`框架的配置。如果使用使用分布式事务必须要使用redis作为分布式缓存。

其中,在同一个微服务集群中,`Rpc:Token`的值必须相同。`Rpc:Port`的缺省值是`2200`,`Rpc:Host`的缺省值为`0.0.0.0`。

在`appsettings.json`中新增如下配置:

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

### 3. 定义一个服务接口

一般地,我们需要将服务接口单独定义在一个项目中,方便被服务消费者引用。

创建一个接口,并通过`[ServiceRoute]`特性标识为该接口是一个应用服务。

```csharp
[ServiceRoute]
public interface IGreetingAppService
{   
    Task<string> Get();
}
```

### 4. 提供者实现服务

创建一个类,通过继承服务接口即可实现接口定义的方法。

```csharp
public class GreetingAppService : IGreetingAppService
{
  public Task<string> Get()
  {
    return Task.FromResult("Hello World");
  }
}
```


### 5. 消费者通过RPC远程调用服务

其他微服务应用只需要通过引用应用服务接口项目,通过接口代理与服务提供者通过`SilkyRpc`框架进行通信。

### 6. Swagger在线文档

运行程序后,打开浏览器,输入`http://127.0.0.1:5000/index.html` 即可查看swagger在线文档,并且通过api进行调试。


## 通过项目模板快速创建应用

silky提供了两个项目模板可以快速的创建应用，开发者可以根据需要选择合适的项目模板来创建应用。

```pwsh

# 以模块的方式创建微服务应用,适用于将所有的应用放在同一个仓库
> dotnet new --install Silky.Module.Template::3.0.0.2

# 以独立应用的方式创建微服务应用,将每个微服务应用单独存放一个仓库
> dotnet new --install Silky.App.Template::3.0.0.2
```

使用项目模板创建微服务应用。

```pwsh

dotnet new -n silky.app --newsln -n Demo

```


## 贡献
- 贡献的最简单的方法之一就是讨论问题（issue）。你也可以通过提交的 Pull Request 代码变更作出贡献。
