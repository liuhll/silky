---
title: 主机
lang: zh-cn
---

## 概念

silky的主机与.net的[主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1)概念一致。是封装应用资源的对象,用于托管应用和管理应用的生命周期。

一般地,Silky会使用到.net两种类型的主机。

### .net的通用主机

如果用于托管普通的业务应用,该微服务模块本身并不需要对直接对集群外部提供访问入口。那么,您可以使用[.net的通用主机](https://docs.microsoft.com/zh-cn/dotnet/core/extensions/generic-host)注册silky服务框架。.net的通用主机无法提供http请求,也无法配置http的请求管道(即:**中间件**)。

但是在注册silky框架后,silky框架会注册dotnetty的服务监听者,并会暴露rpc端口号。但是由于silky框架的安全机制,集群外部并不允许通过`tcp`协议通过rpc端口号直接访问该微服务模块的应用接口。

### .net的web主机

如果您需要访问该服务模块的应用接口,您必须要通过.net的[web主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-3.1)注册silky框架,并配置silky框架的请求管道。这样,web构建的主机通过引用某个微服务的应用接口项目(包)，通过应用接口的代理与微服务集群内部实现rpc通信。

## silky的业务主机类型

### 用于托管业务应用的普通主机

一般地,用于托管普通应用的主机,我们使用.net的通用主机即可,该类型的主机不提供http请求服务,所以也不支持配置请求管道(中间件)。

**为什么silky使用.net通用主机托管普通业务应用?**

对一个微服务应用集群来说,一般地,服务之间的内部通信通过dotnetty通信框架实现的rpc协议进行通信。对于服务之间的rpc通信而言,底层的通信协议是基于dotnetty框架的tcp协议,需要暴露的端口是rpc通信端口,服务之间的rpc通信并不是基于http协议实现的,所以并不需要使用web主机来托管普通微服务主机。

微服务想要对外暴露访问入口,我们只需要通过网关引用微服务模块的应用接口层(包),通过应用接口生成的代理,与微服务主机实例进行rpc通信就可以了。通过这种设计,有效的保证了微服务集群内部的安全性,也方便的微服务集群的统一认证与授权。微服务集群的统一认证和授权,我们只需要在访问入口(网关)统一处理即可。

同时,我们集群内部rpc通信过程中,服务消费者在调用时,指定rpc的`token`，服务消费者通过验证给定的rpc `token`是否一致,从而保证通信的合法性。rpc通信也限制了`token`的设置只能在内部rpc通信时进行设置,这样,集群外部也无法通过rpc端口与普通业务微服务主机进行通信。


创建一个用于托管普通应用的主机非常简单。

#### 1. 创建一个应用台程序

#### 2. 安装`Silky.NormHost`包

#### 3. 注册Silky服务

通过`Host.CreateDefaultBuilder()`创建`IHostBuilder`对象后,调用`RegisterSilkyServices<TModuel>`注册Silky服务框架即可。其中,`TModuel`是您指定的启动模块。启动模块指定了您要依赖的silky模块。`Silky.NormHost`包提供了默认的启动模块`NormHostModule`。

所以,您只需要通过如下代码即可获取到一个支持Silky服务的微服务主机构建者。

```csharp
private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
            .RegisterSilkyServices<NormHostModule>()
        ;
}
```

#### 4. 新增主机必要的配置项

silky框架支持通过`yml`或是`json`格式对框架进行配置。

您可以通过`appsettings.yml`为公共配置项指定配置信息,也可以通过新增`appsettings.${ENVIRONMENT}.yml`文件为指定的环境配置信息。

对silky普通业务主机来说：

A) (**必须的**)您必须要配置的是服务注册中心地址,silky默认使用`zookeeper`作为服务注册中心,支持多服务注册中心地址。同一个集群的注册中心地址使用`,`分割,不同集群的服务注册中心地址使用`;`分割。

B) (**必须的**)您必须要配置redis服务作为分布式锁服务。

C) (**必须的**) silky rpc通信token

D) (**可选的**)您需要为rpc通信服务指定主机地址和端口号。主机地址缺省值为`0.0.0.0`,rpc端口号缺省值为:`2200`。如果您使用项目的方式(非容器化)进行开发和调式应用的话,那么您需要为每一个微服务模块的主机指定一个端口号(端口号不允许重复),如果您使用容器化的方式开发和调式应用的话,那么,端口号可以使用缺省值(每个应用独占一个容器,拥有自己独立的运行环境),但是主机地址不能够设置为`localhost`或是`127.0.0.1`。

E) (**可选的**)使用redis作为分布式缓存服务。如果使用`redis`作为分布式缓存服务的话,那么除了配置缓存服务地址,您还需要将配置项`distributedCache.redis.isEnabled`设置为`true`。

一个最少的的silky主机配置如下所示:

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

主机要实现微服务应用的托管,只需要引用应用服务的实现(即:**对应用层的引用**)即可。在主机启动时,服务主机会自动解析到应用本身,并将应用服务条目的路由注册到服务注册中心,引用了该服务的应用接口层(包)的其他微服务模块(或网关)也可以订阅到服务路由的变化。


### 支持websocket通信的业务主机

Silky框架使用[WebSocketSharp](https://github.com/sta/websocket-sharp)实现ws通信协议。

一个业务微服务模块主机想要支持通过`ws`协议通信，需要依赖`WebSocketModule`模块。在主机启动后,服务主机除了通过`rpc端口`与其他微服务模块进行rpc通信之外,还支持通过`ws端口`与透过代理与前端实现ws通信。

支持websocket通信的业务主机与普通业务应用主机的构建过程一致,也是通过.net的泛型主机进行托管应用。不同的地方只是控制台项目安装的包和指定的启动项目不一样。

您只需要在控制台应用中安装`Silky.WsHost`包。将设置的启动模块设置为:`WsHostModule`即可。这样,一个普通的业务应用主机也支持ws协议透过网关的代理与前端进行通信。

```csharp
private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
            .RegisterSilkyServices<WsHostModule>()
        ;
}
```

要使得一个应用服务支持与前端的`ws`进行会话,那么这个应用服务必须要继承基类`WsAppServiceBase`。更多关于websocket使用知识请参考[ws通信](#)节点。


### 接受Http请求的web主机

一般地,我们使用.net的web主机来构建网关,通过引用各个业务微服务模块的应用接口层(包)，通过silky中间件,解析到相应的服务条目,并通过应用接口的代理实现与具体的某个业务应用主机实例进行通信,完成业务处理。

创建一个接受http请求的silky web主机非常简单。

#### 1. 创建一个空的asp.net web应用

#### 2. 安装`Silky.WebHost`包

#### 3. 注册Silky服务

注册silky服务,并指定`WebHostModule`作为启动模块,并设置`Startup`类。

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
  Host.CreateDefaultBuilder(args)
      .RegisterSilkyServices<WebHostModule>()
      .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

```

#### 4. 在Startup类中设置swagger文档和启用silky请求管道

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo {Title = "Silky Gateway", Version = "v1"});
        c.MultipleServiceKey();
        var applicationAssemblies = EngineContext.Current.TypeFinder.GetAssemblies()
            .Where(p => p.FullName.Contains("Application") ||  p.FullName.Contains("Domain"));
        foreach (var applicationAssembly in applicationAssemblies)
        {
            var xmlFile = $"{applicationAssembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        }
    });
}


public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment() || env.EnvironmentName == "ContainerDev")
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Silky Gateway Demo v1"));
    }
    app.ConfigureSilkyRequestPipeline(); // 启用silky请求管道
}
```

在启用silky请求管道后,会根据您按照的包,自动引用您设置相关中间件。

#### 5. 配置

网关的配置方式与普通业务微服务主机一致,更多配置信息,请参考[配置](#)节点。

#### 6. 为其他微服务模块代理

引用其他微服务应用接口层(包),为其他微服务模块的应用服务生成代理,通过代理与集群内部的rpc通信。

## 自定义启动模块

在介绍构建silky微服务主机时,我们知道在注册silky服务的过程中,必须要指定一个启动模块。一般地,在构建普通业务微服务主机时,我们指定的启动模块是`NormHostModule`;构建支持ws协议的微服务主机时,我们指定的启动模块是`WsHostModule`;构建支持接受Http请求的web主机时,指定的是`WebHostModule`。实际上,我们是通过指定的启动模块指定silky框架依赖的必要模块。

如果您想自己指定依赖的模块(例如:您扩展了一个silky模块项目,希望依赖它),或是在应用启动或是停止时,指定相关的操作,那么您可能就需要自定义启动模块。

自定义启动模块,可以继承`StartUpModule`或是相应的业务微服务的启动模块(例如:`NormHostModule`)。

如果您继承的是`StartUpModule`，那么您必须要显式的指定需要依赖的silky模块。如果您继承的模块基类已经指定了模块的依赖关系(例如:继承`NormHostModule`)，那么基类的模块依赖关系也会被继承。

在启动模块中,您可以通过重写`Initialize`方法实现应用启动时的初始化方法,或是重写`Shutdown`实现应用停止时的方法，也可以通过重写`RegisterServices`通过`ContainerBuilder`注册服务的生命周期。

例如:

```csharp
  [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(MessagePackModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(AutoMapperModule)
  )]
  public class DemoStartUpHostModule : StartUpModule
  {
       public async override Task Initialize(ApplicationContext applicationContext)
        {
            // 应用启动时执行方法
        }

        protected override void RegisterServices(ContainerBuilder builder)
        {
            // 通过ContainerBuilder向ioc容器注册服务
        }

        public async override Task Shutdown(ApplicationContext applicationContext)
        {
             // 应用停止时执行方法
        }
  }
```

在构建主机时指定`DemoStartUpHostModule`作为启动模块。

```csharp
private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
            .RegisterSilkyServices<DemoStartUpHostModule>()
        ;
}
```


