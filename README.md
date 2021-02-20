<p align="center">
  <img height="200" src="./logo.png">
</p>

# lms 微服务框架
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/liuhll/lms/main/LICENSE)
[![Downloads](https://img.shields.io/github/downloads/liuhll/lms/total?label=downloads&logo=github&style=flat-square)](https://img.shields.io/github/downloads/liuhll/lms/total?label=downloads&logo=github&style=flat-square)
[![HitCount](http://hits.dwyl.io/liuhll/lms.svg)](http://hits.dwyl.io/liuhll/lms)
[![Commit](https://img.shields.io/github/last-commit/liuhll/lms)](https://img.shields.io/github/last-commit/liuhll/lms)


Lms是一个旨在通过.net平台快速构建微服务开发的框架。具有稳定、安全、高性能、易扩展、使用方便的特点。lms内部通过[dotnetty](https://github.com/Azure/DotNetty)实现高性能的rpc通信,使用zookeeper作为服务注册中心。RPC通信支持随机轮询、哈希一致性、随机路由等负载均衡算法。

您还可以很方便的与[CAP](https://github.com/dotnetcore/CAP)或是[MassTransit](https://github.com/MassTransit/MassTransit)集成,使用事件总线进行内部通信。

你可以在这里 [Lms docs](#) 看到更多详细资料。

## Getting Started

### LMS框架集成与服务托管

#### 使用通用主机注册和托管LMS服务

使用LMS框架非常简单,您只需要使用.net提供的[通用主机](https://docs.microsoft.com/zh-cn/dotnet/core/extensions/generic-host)注册LMS框架,同时指定启动的模块,在启动的模块中,通过`DependsOn`特性依赖必要的组件。

通用主机注册LMS框架主要用于一般情况下对服务的托管,配置的`token`保证的集群通信的安全性,避免用户直接通过RPC端口访问集群内部，

(1) 只有请求来源于网关,才被认为是合法的请求。集群外部无法通过rpc端口与主机直接通信。

(2) 服务内部通信过程中,同一个集群只有配置的token一致, 通信才会被被认为是合法的。 

1. 注册LMS服务

```csharp
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .RegisterLmsServices<NormModule>() //注册lms服务，并指定启动的模块
                ;

        }
    }
```

2. 指定启动模块

```csharp
    [DependsOn(typeof(ZookeeperModule), 
      typeof(DotNettyTcpModule), 
      typeof(RpcProxyModule),
      typeof(MessagePackModule))]
    public class NormModule : LmsModule
    {
        
    }
```

启动模块必须要继承`LmsModule`基类，通过`DependsOn`特性指定依赖的模块组件，一般的,您需要依赖服务注册中心组件(`ZookeeperModule`)、和通信框架组件(`DotNettyTcpModule`)、Rpc通信代理组件(`RpcProxyModule`),也可以指定编解码组件(`MessagePackModule`或是`ProtoBufferModule`),如果未指定编解码组件,则默认使用json作为rpc内部的通信编解码格式。同一个集群内部，必须要保证使用的编解码一致。

在启动模块中,您也可以通过重写`RegisterServices`来注册需要注入ioc的类，通过重写`Initialize`方法在应用启动时执行初始化方法,重写`Shutdown`方法在应用结束时执行释放资源的方法。

3. 配置

您可以使用`json`或是`yml`格式对服务进行配置。如下所示,列举了lms服务必要的最少配置。

```yml
rpc:
    host: 0.0.0.0 # 主机地址
    rpcPort: 2201 # rpc通信端口号
    token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW # token令牌
registrycenter:
    connectionStrings: 127.0.0.1:2181 # 服务注册中心地址
    registryCenterType: Zookeeper # 服务注册中心类型
  
```

4. 完成主机构建后,您可以引用各个微服务模块的应用接口,或是托管服务自身的应用服务。集群内部使用dotntty实现的RPC框架进行通信。

#### 使用Web主机注册和托管LMS服务

您可以使用.net的[web主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host)来注册和托管lms服务,通过这种形式构建的服务主机,可以通过引用各个微服务模块的应用接口,通过web主机指定的http端口对外提供访问。web主机可以通过应用接口生成的代理与微服务集群内部各个服务主机进行通信。

通过web主机注册LMS服务时,一般不需要实现应用接口(即，不需要托管应用服务),只需要引用各个微服务模块的应用接口,通过HTTP端口提供一个与集群外部通信的方式。

1. 注册LMS服务

```csharp
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .RegisterLmsServices<GatewayModule>()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
```

2. 启动模块

```csharp
    [DependsOn(typeof(RpcProxyModule),
        typeof(ZookeeperModule),
        typeof(HttpServerModule),
        typeof(DotNettyModule),
        typeof(MessagePackModule)
        )]
    public class GatewayModule : LmsModule
    {
    }

```

与通用主机的指定的启动模块相比,需要额外依赖`HttpServerModule`模块。

3. StartUp类

在[StartUp类](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/startup)中,您可以通过`ConfigureServices`配置服务的注入,以及在`Configure`方法中配置Http请求中间件。例如: 您可以在StartUp类中配置mvc路由、SwaggerAPI稳定等。

需要注意的是必须在`Configure`中必须要配置`app.ConfigureLmsRequestPipeline()`,只有这样Http请求才可以通过lms框架预先设置的中间件。

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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Lms Gateway", Version = "v1"});
                c.MultipleServiceKey();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lms Gateway Demo v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.ConfigureLmsRequestPipeline();
        }
    }
```

### 应用接口与服务条目

在一个微服务模块中,您可以通过定义应用接口来提供相应的服务条目,应用接口只需要通过`ServiceRoute`特性进行描述。每个应用接口定义的方法都会生成一个服务条目,应用接口就相当于MVC的Controller,服务条目相当于Action。

一般的,我们会将应用接口单独定义在一个独立的程序集，应用接口的实现定义在另外一个单独的程序集。应用接口程序集可以被其他微服务模块引用,在其他微服务模块中可以通过引用的应用接口生成代理,通过应用接口代理可以方便与服务提供者进行rpc通信。

```csharp
    [ServiceRoute]
    public interface ITestAppService
    {
       // 应用接口
       // 每个方法都会生成一个服务条目
        [HttpGet("{name:string}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("ITestAppService.TestOut", "name:{0}")]
        Task<TestOut> Get([HashKey][CacheKey(0)] string name);
    }

```

更多应用接口的说明和配置请[参考](#)。

### 配置

Lms支持通过`json`或是`yml`格式的对框架进行配置。一般的,您可以通过`appsettings.json`或是`appsettings.yml`文件对系统进行配置，并且您可以通过`appsettings.{Environment}.yml`或是`appsettings.{Environment}.json`指定在不同的运行环境中加载相应的环境变量。有关详细信息，请参阅[在 ASP.NET Core 中使用多个环境](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/environments)。

#### RPC通信参数配置

| 配置项 | 名称 | 缺省值 | 备注 |
|:------|:------|:------|:------|
| Host | 主机地址 | 0.0.0.0 | 缺省配置则自动获取当前服务器的IP为rpc服务IP |
| RpcPort | Rpc通信端口 | 2200 | 指定的rpc通信端口号 |
| MqttPort | mqtt协议通信端口 | 2300 | 当前暂未实现mqtt通信协议 |
| UseLibuv | 使用libuv  | 2300 | dotnetty通信是否使用libuv网络库,缺省值为`true` |
| IsSsl | 使用Ssl加密  | false |  |
| SslCertificateName | Ssl证书名称  |  | Ssl证书的相对路径全名称 |
| SslCertificatePassword | Ssl证书密码  |  |  |
| SoBacklog | dotnetty SoBacklog  | 8192 | 过大或过小会影响性能 |
| RemoveUnhealthServer | 移除不健康服务 | true | 是否从服务注册中心移除不健康的服务提供者 |


#### 服务注册中心配置

| 配置项 | 名称 | 缺省值 | 备注 |
|:------|:------|:------|:------|
| RegistryCenterType | 服务注册中心类型 | RegistryCenterType.Zookeepe | 当前仅实现了zookeeper作为服务注册中心 |
| ConnectionTimeout | 连接超时 | 20 | 单位(秒)  |
| SessionTimeout | session会话超时 | 30 | 单位(秒)  |
| OperatingTimeout | 操作超时 | 60 | 单位(秒)  |
| ConnectionStrings | 服务注册中心地址 |  | 支持多服务注册中心,通一个集群的地址使用逗号(,)分割，多个服务注册中心使用(;)进行分割。例如:`zookeeper1:2181,zookeeper1:2182,zookeeper1:2183;zookeeper2:2181,zookeeper2:2182,zookeeper2:2183` |
| RoutePath | 服务条目的路由地址 | /services/serviceroutes | 当服务启动时会自动注册路由信息到服务注册中心 |
| MqttPtah | Mqtt协议的服务条目的路由地址 | /services/serviceroutes | 暂未实现 |
| HealthCheckInterval |  | | 暂未实现心跳检查 |


##### 服务治理相关配置

可以通过`Governance`配置节点统一的rpc通信过程进行配置,但是该配置的可以被服务条目的`GovernanceAttribute`特性进行改写。

| 配置项 | 名称 | 缺省值 | 备注 |
|:------|:------|:------|:------|
| ShuntStrategy | 负载分流策略 | | rpc通信过程中的服务路由的地址选择器 |
| ExecutionTimeout | 执行超时时间 | -1 | 单位(ms),0或-1表示不会超时,建议值:1000 |
| MaxConcurrent | 允许的最大并发量 | 100 | 超过最大的并发量则会路由到其他的服务提供者实例,如果不存在则会发生熔断保护 |
| FuseSleepDuration | 熔断休眠时间 | 60 | 单位(s)，熔断期间不会被路由到服务提供者实例 |
| FuseProtection | 是否开启熔断保护 | true | 服务提供者无法到达时,开启熔断保护则不会立即将该服务提供者的实例标识为不健康,但是在熔断休眠时间内该实例不会被路由 |
| FuseTimes |  | | 熔断几次之后标识服务提供者实例不健康 |
| FailoverCount | 故障转移次数  | | 服务提供者不可达时,允许的重新路由的次数,缺省值为0,则服务提供者实例有几个,则会重试几次 |

更对关于服务通信的治理请查看[服务治理](#)节点相关文档。

### 缓存拦截

在rpc通信过程中,您可以使用缓存拦截。通过使用缓存拦截,可以大大提升系统的性能。在一个应用接口方法配置了缓存拦截,那么在方法执行前,如果存在缓存,则会从缓存中取出数据。

您可以在服务条目的方法上通过如下特性对缓存拦截进行配置。

1. `GetCachingInterceptAttribute` 优先从缓存中读取缓存数据,如果缓存中不存在,才执行本地或是远程方法。

2. `UpdateCachingInterceptAttribute` 执行本地或远程方法并更新缓存数据。

3. `RemoveCachingInterceptAttribute` 执行本地或远程方法,并删除相应的缓存数据。

更对缓存拦截的使用和配置请查看[缓存拦截文档](#)。