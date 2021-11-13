---
title: 模块
lang: zh-cn
---

## 模块的定义和类型

在silky框架,模块是应用程序用于服务注册、初始化任务、释放资源的单位,被定义为一个程序集。模块具有依赖关系,通过`DependsOn`特性来确定模块之间的依赖关系。

silky框架存在**两种类型**的模块:

1. 开发者通过继承`SilkyModule`就可以定义一个普通模块类;
2. 也可以通过继承`StartUpModule`定义一个服务注册启动模块类。


例如:

普通模块类

```csharp
// 普通模块类,启动模块类必须要直接或间接的依赖该模块
[DependsOn(typeof(RpcModule))]
public class CustomModule : SilkyModule
{
}

```

启动模块类

```csharp
// 启动模块类，只有该类型的模块才可以被允许在构建服务中被指定为启动模块
[DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(AutoMapperModule)
)]
public class NormHostModule : StartUpModule
{
}

```
::: tip

1. 开发者想要执行一个模块,需要在微服务时指定该模块,或是通过`DependsOn`特性直接或是间接的依赖该模块。

2. 只有启动模块类才可以在服务服务注册时指定该模块为注册的启动模块。
:::

## 在模块中注册服务

模块提供了两个服务注册的API,一是通过`ServiceCollection`实现服务注册,二是通过`ContainerBuilder`实现服务注册。

### 通过`ServiceCollection`实现服务注册

开发者通过重写`ConfigureServices`方法,可以通过`IServiceCollection`实现服务注册，例如:

```csharp
public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddOptions<RpcOptions>()
        .Bind(configuration.GetSection(RpcOptions.Rpc));
    services.AddOptions<GovernanceOptions>()
        .Bind(configuration.GetSection(GovernanceOptions.Governance));
    services.AddOptions<WebSocketOptions>()
        .Bind(configuration.GetSection(WebSocketOptions.WebSocket));
    services.AddDefaultMessageCodec();
    services.AddDefaultServiceGovernancePolicy();
}
```

### 通过`ContainerBuilder`实现服务注册

`ContainerBuilder` 是 [Autofac](https://github.com/autofac/Autofac) 提供服务注册的类,开发者可以通过重写`RegisterServices`方法使用`ContainerBuilder`提供的API实现服务注册。使用`ContainerBuilder`注册服务的一个优势是可以注册命名的服务。

```csharp
protected override void RegisterServices(ContainerBuilder builder)
{
    builder.RegisterType<DefaultExecutor>()
        .As<IExecutor>()
        .InstancePerLifetimeScope()
        .AddInterceptors(
            typeof(CachingInterceptor)
        )
        ;
}
```

## 使用模块初始化任务

在应用程序启动过程中,开发者可以重写`Initialize`方法来实现模块的初始化任务。开发者可以通过`applicationContext.ServiceProvider`属性来解析注册的服务。

```csharp
public override async Task Initialize(ApplicationContext applicationContext)
{
    var serverRouteRegister =
        applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
    await serverRouteRegister.RegisterServer();
}
```

## 使用模块释放资源

在应用程序正常停止时,通过重写`Shutdown`方法来实现模块停止时需要执行的方法,例如：释放资源等。

```csharp
public override async Task Shutdown(ApplicationContext applicationContext)
{
    var serverRegister =
        applicationContext.ServiceProvider.GetRequiredService<IServerRegister>();
    await serverRegister.RemoveSelf();
}
```

## 模块的依赖关系

silky框架的模块通过`DependsOn`特性指定模块的依赖关系,silky框架支持通过直接或是间接的依赖模块。例如: `NormHostModule`模块依赖了`DotNettyTcpModule`模块,`DotNettyTcpModule`模块依赖了`RpcModule`模块,微服务注册时指定`NormHostModule`为启动模块。那么根据模块依赖关系,`RpcModule`模块会被应用加载,并先于`DotNettyTcpModule`和`NormHostModule`执行服务注册方法和初始化方法。

开发者只需要通过`DependsOn`特性在类直接就可以指定该模块依赖的模块,在应用启动过程中,会根据模块的依赖关系进行排序。并完成服务注册方法和指定的初始化方法。

例如,`NormHostModule`的模块依赖关系如下所示:

```csharp
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(AutoMapperModule)
    )]
    public class NormHostModule : StartUpModule
    {
    }
```


## 构建主机时指定启动模块

开发者如果自定义了模块,那么,需要在构建微服务主机时,指定启动模块。

例如:

```csharp

private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
            .RegisterSilkyServices<NormHostModule>() //指定启动的模块，silky框架约束了该模块类型必须为启动模块类(StartUpModule)
        ;
}

```

一般地,开发者在构建默认主机时,并不需要指定启动模块。构建的默认主机,已经根据构建的主机类型，指定了默认的启动模块。例如,使用`ConfigureSilkyWebHostDefaults`构建silky主机时,已经指定了`DefaultWebHostModule`作为其中模块。

如果开发者有自定义模块时,同时也需要自定义一个启动模块,通过该启动模块依赖开发者自定义的模块和 silky 框架定义的模块，达到服务注册和初始化任务的目的。

例如:

```csharp
[DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(AutoMapperModule),
        typeof(CustomModule),
)]
public class CustomStartHostModule : StartUpModule
{
}

```

为了方便开发者,silky框架根据构建主机的类型,已经创建了多种启动模块,该类型的启动模块已经定义好了该模块必须的依赖的模块:

1. 通过web主机构建微服务应用的`WebHostModule`模块
2. 通过通用主机构建微服务应用的`GeneralHostModule`模块
3. 构建websocket服务主机的应用的`WebSocketHostModule`模块
4. 构建只能作为服务消费者网关应用的`GatewayHostModule`模块

开发者可以选择继承如上的启动模块,并且配置Host主机提供API就可以构建相应的主机。