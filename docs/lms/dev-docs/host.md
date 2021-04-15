---
title: 主机
lang: zh-cn
---

## 概念

lms的主机与.net的[主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1)概念一致。是封装应用资源的对象,用于托管应用和管理应用的生命周期。

一般地,Lms会使用到.net两种类型的主机。

### .net的通用主机

如果用于托管普通的业务应用,该微服务模块本身并不需要对直接对集群外部提供访问入口。那么,您可以使用[.net的通用主机](https://docs.microsoft.com/zh-cn/dotnet/core/extensions/generic-host)注册lms服务框架。.net的通用主机无法提供http请求,也无法配置http的请求管道(即:**中间件**)。

但是在注册lms框架后,lms框架会注册dotnetty的服务监听者,并会暴露rpc端口号。但是由于lms框架的安全机制,集群外部并不允许通过`tcp`协议通过rpc端口号直接访问该微服务模块的应用接口。

### .net的web主机

如果您需要访问该服务模块的应用接口,您必须要通过.net的[web主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-3.1)注册lms框架,并配置lms框架的请求管道。这样,web构建的主机通过引用某个微服务的应用接口项目(包)，通过应用接口的代理与微服务集群内部实现rpc通信。

## lms的业务主机类型

### 用于托管业务应用的普通主机

一般地,用于托管普通应用的主机,我们使用.net的通用主机即可,该类型的主机不提供http请求服务,所以也不支持配置请求管道(中间件)。

为什么选择.net通用主机托管普通业务应用?

对平台应用服务来说,这是应该的,因为我们有必要让一个微服务集群只需要一个对外部的(Http访问)访问入口。

创建用于托管普通应用的主机非常简单。

#### 1. 创建一个应用台程序

#### 2. 安装`Silky.Lms.NormHost`包

#### 3. 注册LMS服务

通过`Host.CreateDefaultBuilder()`创建`IHostBuilder`对象后,调用`RegisterLmsServices<TModuel>`注册Lms服务框架即可。其中,`TModuel`是您指定的启动模块。启动模块指定了您要依赖的lms模块。`Silky.Lms.NormHost`包提供了默认的启动模块`NormHostModule`。

所以,您只需要通过如下代码即可获取到一个支持Lms服务的微服务主机构建者。

```csharp
private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
            .RegisterLmsServices<NormHostModule>()
        ;
}
```

#### 4. 新增主机必要的配置项

lms框架支持通过`yml`或是`json`格式对框架进行配置。

您可以通过`appsettings.yml`为公共配置项指定配置信息,也可以通过新增`appsettings.${ENVIRONMENT}.yml`文件为指定的环境配置信息。

对lms普通业务主机来说。

1) (**必须的**)您必须要配置的是服务注册中心地址,lms默认使用`zookeeper`作为服务注册中心,支持多服务注册中心地址。同一个集群的注册中心地址使用`,`分割,不同集群的服务注册中心地址使用`;`分割。

2) (**必须的**)您必须要配置redis服务作为分布式锁服务。

3) (**必须的**) lms rpc通信token

3) (**可选的**)您需要为rpc通信服务指定主机地址和端口号。主机地址缺省值为`0.0.0.0`,rpc端口号缺省值为:`2200`。如果您使用项目的方式(非容器化)进行开发和调式应用的话,那么您需要为每一个微服务模块的主机指定一个端口号(端口号不允许重复),如果您使用容器化的方式开发和调式应用的话,那么,端口号可以使用缺省值(每个应用独占一个容器,拥有自己独立的运行环境),但是主机地址不能够设置为`localhost`或是`127.0.0.1`。

4) (**可选的**)使用redis作为分布式缓存服务。如果使用`redis`作为分布式缓存服务的话,那么除了配置缓存服务地址,您还需要将配置项`distributedCache.redis.isEnabled`设置为`true`。

一个最少的的lms主机配置如下所示:

```yml
rpc:
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
registrycenter:
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186
  registryCenterType: Zookeeper
lock:
  lockRedisConnection: 127.0.0.1:6379,defaultDatabase=1
```

#### 5. 运行主机

通过`IHostBuilder`的实例对象的`Build()`方法获取到主机对象后,调用`RunAsync()`方法即可运行主机。

```csharp
public static async Task Main(string[] args)
{
    await CreateHostBuilder(args).Build().RunAsync();
}
```

#### 5. 托管应用

主机要实现微服务应用的托管,值需要引用应用服务的实现(即:对应用层的引用)即可。在主机启动时,服务主机会自动解析到应用本身,并将应用服务条目的路由注册到服务注册中心,引用了该服务的应用接口层(包)的其他微服务模块(或网关)也可以订阅到服务路由的变化。


### 支持websocket通信的平台主机

### 接受Http请求的web主机

## 自定义启动模块


