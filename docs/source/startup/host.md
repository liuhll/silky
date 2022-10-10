---
title: 主机的构建
lang: zh-cn
---

## 主机的概念

首先，我们来了解主机的概念。在[Asp.net Core主机文档中](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-6.0),将主机定义为: **主机是封装应用资源的对象，将应用的所有相互依赖资源包括在一个对象中可控制应用启动和正常关闭**。换句话说,就是用于托管和管理应用资源和应用生命周期的。

在.net 中,有两种类型的主机,一种是[泛型主机](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-6.0)，一种是[Web主机](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0)。区别主要在于**Web主机**提供了web容器和配置http请求处理管道的能力(所以web主机在启动后就有一个`http端口`和可以通过`StartUp`类配置http中间件),而通用主机并没有这个能力。

对于silky来说,由silky框架开发的微服务集群存在两种通信方式:

1.  接受外部的http请求，并且通过webapi路由到相应的服务条目,然后通过本地或远程执行器执行对于的服务条目方法。

2. 服务与服务内部之间通过dotnetty实现的rpc进行网络通信。

所以,根据不同的使用场景,我们可以使用不同的主机来托管应用。

对于 *场景1* ,我们只能选择通过**Web主机**来托管应用,因为他必须提供Http服务的能力,是集群内部对外提供webapi访问的入口应用,一般可以用于构建网关,当然,如果用于托管业务应用也可以，这样的话,该业务应用也能直接对外部提供webapi服务。

但是,一般场景下,普通的业务微服务我们并不需要它直接对外部提供webapi服务。所以,更多通信场景是*场景2*。 所以,对于一般的业务微服务而言,我们可以使用他 **通用主机** 来构建微服务应用。

## 注册Silky微服务应用

我们在[silky文档首页](https://docs.silky-fk.com/)看到,构建一个最简单silky微服务应用,只需要通过如下一行简单的代码就可以做到。

```csharp
 private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
            .ConfigureSilkyGeneralHostDefaults();
            
}
```

我们通过包 **Silky.Agent.Host** 提供的 `HostBuilderExtensions`提供的扩展方法来构建Silky微服务应用。**Silky.Agent.Host** 包依赖了用于构建Silky微服务应用其他的必要的包。

`HostBuilderExtensions`通过实现`IHostBuilder`的扩展方法来注册Silky微服务应用。在[HostBuilderExtensions](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Agent.Host/HostBuilderExtensions.cs)类中,提供了诸如: `ConfigureSilkyWebHostDefaults`、`ConfigureSilkyGateway`、`ConfigureSilkyGeneralHostDefaults`等`IHostBuilder`的扩展方法。在这些方法中,无论哪个方法,我们看到,核心的代码就是 `hostBuilder.RegisterSilkyServices<T>()`,下面,我们深入了解 `hostBuilder.RegisterSilkyServices<T>()`究竟完成了什么工作。

在**Silky.Core**包中,我们通过[HostBuilderExtensions](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/HostBuilderExtensions.cs)提供的扩展方法`RegisterSilkyServices`实现了服务引擎(`IEngine`)的构建、模块的依赖与注册、服务的依赖与注册、配置文件的装载、模块的顺序执行等工作。

```csharp
public static IHostBuilder RegisterSilkyServices<T>(this IHostBuilder builder)
  where T : StartUpModule
{
    IEngine engine = null;
    IServiceCollection services = null;
    builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureServices((hostBuilder, sc) => // (2) 服务注册和服务引擎构建
        {
            engine = sc.AddSilkyServices<T>(hostBuilder.Configuration,
                hostBuilder.HostingEnvironment);
            services = sc;
        })
        .ConfigureContainer<ContainerBuilder>(builder => // (3)通过ContainerBuilder实现服务依赖注册
        {
            engine.RegisterModules(services, builder);
            engine.RegisterDependencies(builder);
        })
        .ConfigureAppConfiguration((hosting, config) => // (1)装载配置文件、添加环境变量
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true,
                    true);
            // Adds YAML settings later
            config.AddYamlFile("appsettings.yml", optional: true, true)
                .AddYamlFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.yml", optional: true,
                    true)
                .AddYamlFile("appsettings.yaml", optional: true, true)
                .AddYamlFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.yaml", optional: true,
                    true);
            // add RateLimit configfile
            config.AddJsonFile("ratelimit.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"ratelimit.{hosting.HostingEnvironment.EnvironmentName}.json", optional: true,
                    true);
            config.AddYamlFile("ratelimit.yml", optional: true, reloadOnChange: true)
                .AddYamlFile($"ratelimit.{hosting.HostingEnvironment.EnvironmentName}.yml", optional: true,
                    true)
                .AddYamlFile("ratelimit.yaml", optional: true, reloadOnChange: true)
                .AddYamlFile($"ratelimit.{hosting.HostingEnvironment.EnvironmentName}.yaml", optional: true,
                    true);
            config.AddEnvironmentVariables();
        })
        ;
    return builder;
}
```

1. 该方法约束了泛型参数`T`必须为启动模块`StartUpModule`类型。

2. 将服务提供者工厂替换为`AutofacServiceProviderFactory`，这样,我们就可以通过[Autofac](https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html#asp-net-core-3-0-and-generic-hosting)来实现服务的依赖注入。

3. 通过`UseContentRoot`指定了项目的根目录。

4. `IHostBuilder`有三个核心的配置方法: (1) `ConfigureAppConfiguration` (2) `ConfigureServices` (3) `ConfigureContainer`; 在应用启动时,将按`ConfigureAppConfiguration` --> `ConfigureServices` --> `ConfigureContainer` 依次执行。

4.1 在执行`ConfigureAppConfiguration`方法时,主要完成加载本地配置文件和环境变量;

4.2 在执行`ConfigureServices`方法时，通过`IServiceCollection`的扩展方法`AddSilkyServices<T>()`实现必要的服务注册和 [**服务引擎(IEngine)**](/source/startup/engine.html) 的构建;

4.3 在执行`ContainerBuilder`方法时，主要通过Autofac的`ContainerBuilder`实现服务的依赖注册;

5. 在完成上述指定的方法后,主机接下来将会执行后台任务`InitSilkyHostedService`,并根据模块的依赖顺序,依次执行各个模块的启动方法。完成服务以及服务条目的发现、向服务注册中心注册服务信息以及启动rpc消息监听器等工作。


