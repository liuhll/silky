---
title: 模块
lang: zh-cn
---

## 模块的定义


一般地,开发者如果想要在一个自定义的程序集(包)中注册相关的服务，或者在应用初始化或停止时执行一段自定义的代码，那么您可能需要将该程序集(包)定义为一个模块。

silky框架存在**两种类型**的模块:
1. 开发者通过继承`SilkyModule`就可以定义一个普通模块类;
2. 也可以通过继承`StartUpModule`定义一个服务注册启动模块类。


例如:

普通类型模块类

```csharp
// 普通类型模块类
public class CustomHostModule : SilkyModule
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


## 微服务注册时指定启动模块

在构建微服务时,需要指定启动的模块。

例如:
```csharp

private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
            .RegisterSilkyServices<NormHostModule>() //指定启动的模块，silky框架约束了该模块类型必须为启动模块类(StartUpModule)
        ;
}

```

一般地,开发者可以根据微服务类型选择注册silky框架默认的模块,silky框架提供如下三个服务启动模块(`NormHostModule`、`WebHostModule`、`WsHostModule`),silky框架给定的默认启动模块会帮用户指定该类型的微服务应用需要依赖哪些模块。

开发者如果需要自定义模块,那么，相应的,开发者需要在模块依赖关系中声明依赖该模块。所以,一般地,如果您需要自定义模块,那么相应的,您可能需要自定义服务注册启动模块。

您可以通过继承`StartUpModule`或是继承该silky框架提供的默认启动服务模块(`NormHostModule`、`WebHostModule`、`WsHostModule`),定义您的启动模块,并在您定义的启动模块中依赖您的自定义模块。

例如:

```csharp
[DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(AutoMapperModule),
        typeof(CustomHostModule),
)]
public class CustomNormHostModule : StartUpModule
{
}

// 或是

[DependsOn(typeof(CustomHostModule)
)] // 模块的依赖关系也会被继承
public class CustomNormHostModule : NormHostModule
{
}

```

## 使用`ContainerBuilder `注册服务

在定义的模块中,开发者可以通过重写`RegisterServices`方法,通过`ContainerBuilder`对象进行服务注册。

例如：

```csharp
protected override void RegisterServices(ContainerBuilder builder)
{
    builder.RegisterType<MessagePackTransportMessageDecoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
    builder.RegisterType<MessagePackTransportMessageEncoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
}
```

## 应用初始化方法和停止方法

开发者可以通过重新自定义模块的`Initialize`方法在微服务应用启动时执行该模块指定的代码,通过重写`Shutdown`方法在应用停止时执行该模块指定的代码。

```csharp
public virtual Task Initialize([NotNull]ApplicationContext applicationContext)
{
    // 微服务应用启动时,执行该段代码
    return Task.CompletedTask;
}

public virtual Task Shutdown([NotNull]ApplicationContext applicationContext)
{
    // 微服务应用停止时,执行该段代码
    return Task.CompletedTask;
}
```
