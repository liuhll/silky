---
title: silky微服务简介
lang: zh-cn
---

## 代理主机

silky微服务定义了三种类型的代理主机,开发者可以根据需要选择合适的silky代理主机托管微服务应用。代理主机定义了一个`Startup`模块，该模块给出了使用该种类型主机所必须依赖的模块。

### 通用代理主机

该类型的主机一般用于托管业务应用,服务内部之间通过rpc进行通信,不支持与微服务集群与外部进行通信,web代理主机可以通过引用该类型的微服务的应用接口,通过应用接口生成的代理与该微服务进行通信。该类型的微服务使用.net的通用主机进行托管引用。定义的`Startup`模块如下所示:

```csharp
  [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(TransactionRepositoryRedisModule)
    )]
    public abstract class GeneralHostModule : StartUpModule
    {
    }
```

开发者如果需要创建一个微服务应用,只需要在创建一个控制台应用,通过nuget包管理工具安装`Silky.Agent.GeneralHost`包,在主函数中注册`SilkyServices`,并指定启动模块即可。

```csharp

   public static async Task Main(string[] args)
   {
       await CreateHostBuilder(args).Build().RunAsync();
   }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .RegisterSilkyServices<AccountHostModule>()
        ;
    }

```

开发者通过继承`GeneralHostModule`模块定义启动模块。可以通过`DependsOn`依赖自定义的模块或是Silky提供的模块。

启动模块如下所示:

```csharp
// 
//  [DependsOn(typeof(SilkySkyApmAgentModule),
//         typeof(JwtModule),
//         typeof(MessagePackModule))]
public class AccountHostModule : GeneralHostModule
{

}
```

### web代理主机

该类型的主机可以通过http端口与外部通过http协议进行通信,通过引用其他业务微服务应用的应用接口,根据路由模版生成restful风格的webapi,开发者可以通过配置生成在线的`swagger`文档。直观的看到在线api文档和进行调试。生成的swagger文档可以根据引用的应用接口进行分组。

![silky-ms1.png](/assets/imgs/silky-ms1.png)


web代理主机预定义的`Startup`模块指定了web代理主机必须依赖的silky模块,如下所示:

```csharp
    [DependsOn(typeof(RpcProxyModule),
       typeof(ZookeeperModule),
       typeof(SilkyHttpCoreModule),
       typeof(DotNettyModule),
       typeof(ValidationModule),
       typeof(FluentValidationModule),
       typeof(RedisCachingModule)
   )]
    public abstract class WebHostModule : StartUpModule
    {

    }
```

该类型的主机一般用于网关，提供了外部与微服务集群进行通信的桥梁,该类型的主机使用.net的web主机进行托管应用。开发者可以创建一个aspnetcore项目,通过安装`Silky.Agent.WebHost`包即可创建web代理主机，需要同时指定启动模块和`Startup`类。

```csharp
    public async static Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .RegisterSilkyServices<GatewayHostModule>()
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        return hostBuilder;
    }
```

web代理主机的启动模块需要继承`WebHostModule`，启动模块`GatewayHostModule`如下所示:

```csharp
public class GatewayHostModule : WebHostModule
{
    
}
```

需要在`Startup`类注册`Silky`请求管道，`Startup`类如下所示:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.EnvironmentName == "ContainerDev")
        {
            app.UseDeveloperExceptionPage();
        }
        app.ConfigureSilkyRequestPipeline();
    }
}
```

### websocket代理主机

websocket代理主机与通用代理主机基本一致,websocket代理主机具体提供ws服务的能力,web主机可以通过ws代理与websocket代理主机的ws服务进行通信。

websocket代理主机的启动模块如下所示:

```csharp
    [DependsOn(typeof(ZookeeperModule),
        typeof(DotNettyTcpModule),
        typeof(RpcProxyModule),
        typeof(TransactionTccModule),
        typeof(WebSocketModule),
        typeof(ValidationModule),
        typeof(FluentValidationModule),
        typeof(RedisCachingModule),
        typeof(TransactionRepositoryRedisModule)
        )]
    public abstract class WebSocketHostModule : StartUpModule
    {
    }
```

开发者可以通过`WebSocket`配置节点对ws服务进行配置,ws服务的默认端口为`3000`，但是一般地,与ws服务建立握手时,不应该与ws服务直接进行握手,而是应该通过web代理主机的代理中间件进行握手，所以如果开发者使用ws服务,必须要在web代理主机安装`Silky.WebSocket.Middleware`。

ws服务的创建与通用代理主机的创建一致,只需要将启动模块继承的基类修改为`WebSocketHostModule`即可。

ws服务的定义如下:

```csharp
    public class WsTestAppService : WsAppServiceBase, IWsTestAppService
    {
        public async Task Echo(string businessId, string msg)
        {
            if (BusinessSessionIds.TryGetValue(businessId, out var sessionIds))
            {
                foreach (var sessionId in sessionIds)
                {
                    SessionManager.SendTo($"message:{msg},sessionId:{sessionId}", sessionId);
                }
            }
            else
            {
                throw new BusinessException($"不存在businessId为{businessId}的会话");
            }
        }
    }
```

需要注意的时,在建立握手过程中,必须要指定`hashkey`从而保证每次回话的微服务实例都是同一个,更多关于ws服务[请查看](#)。

## 分布式事务

silky微服务使用拦截器和filter实现了TCC分布式事务,在tcc分布式事务过程中,将事务参与者的调用参数作为undolog日志保存到数据仓库中(当前实现了redis作为undo日志的数据存储器),并在后台执行作业检查分布式事务的执行情况,从而保证数据的最终一致性。

### 分布式事务的使用

1. 在应用接口中通过`[Transaction]`特性进行标识该接口是一个分布式应用方法。

```csharp
[Transaction]
Task<GetOrderOutput> Create(CreateOrderInput input);
```

2. 应用服务实现通过`[TccTransaction]`特性标识,并指定`ConfirmMethod`方法和`CancelMethod`,指定实现的`ConfirmMethod`方法和`CancelMethod`必须为`public`，方法参数与应用实现方法的保持一致。try方法如果需要向`ConfirmMethod`方法和`CancelMethod`传递参数通过`RpcContext.Context`进行。

```csharp
        [TccTransaction(ConfirmMethod = "OrderCreateConfirm", CancelMethod = "OrderCreateCancel")]
        [UnitOfWork]
        public async Task<GetOrderOutput> Create(CreateOrderInput input)
        {
            var orderOutput = await _orderDomainService.Create(input);
            return orderOutput;
        }

        [UnitOfWork]
        public async Task<GetOrderOutput> OrderCreateConfirm(CreateOrderInput input)
        {
            var orderId = RpcContext.Context.GetAttachment("orderId");
            var order = await _orderDomainService.GetById(orderId.To<long>());
            order.Status = OrderStatus.Payed;
            order.UpdateTime = DateTime.Now;
            order = await _orderDomainService.Update(order);
            return order.Adapt<GetOrderOutput>();
        }
        
        [UnitOfWork]
        public async Task OrderCreateCancel(CreateOrderInput input)
        {
            var orderId = RpcContext.Context.GetAttachment("orderId");
            if (orderId != null)
            {
                // await _orderDomainService.Delete(orderId.To<long>());
                var order = await _orderDomainService.GetById(orderId.To<long>());
                order.Status = OrderStatus.UnPay;
                await _orderDomainService.Update(order);
            }
        }
```

## 服务定义与RPC通信

### 应用接口的定义

silky的服务定义非常简单,在这里的服务指的是应用服务,与传统MVC的`Controller`的概念一致。

您只需要在一个业务微服务应用中,新增应用接口层,一般地,我们可以命名为`Project.IApplication`或是`Project.Application.Contracts`,并新增应用接口,在应用接口中通过`[ServiceRoute]`特性进行标识,并在`Project.Application`项目中实现该接口。

您可以通过`[ServiceRoute]`指定该应用服务的路由模板, 以及是否允许多个实现。

例如:

```csharp

namespace Silky.Account.Application.Contracts.Accounts
{
    /// <summary>
    /// 账号服务
    /// </summary>
    [ServiceRoute]
    public interface IAccountAppService
    {
        /// <summary>
        /// 新增账号
        /// </summary>
        /// <param name="input">账号信息</param>
        /// <returns></returns>
        Task<GetAccountOutput> Create(CreateAccountInput input);
    }
}

```
在应用层中实现该接口:

```csharp
namespace Silky.Account.Application.Accounts
{
    public class AccountAppService : IAccountAppService
    {
        private readonly IAccountDomainService _accountDomainService;


        public AccountAppService(IAccountDomainService accountDomainService)
        {
            _accountDomainService = accountDomainService;
        }

        public async Task<GetAccountOutput> Create(CreateAccountInput input)
        {
            var account = await _accountDomainService.Create(input);
            return account.Adapt<GetAccountOutput>();
        }
    }
}
```

### RPC通信

应用接口可以被其他微服务应用或是被网关应用引用。其他微服务可以通过应用接口生成代理,并通过内部实现的rpc框架与该微服务进行通信。silky的rpc通信支持tcc方式的分布式事务，详情见上节。

应用接口被网关引用,web host主机可以通过该应用服务接口生成相应的webapi,并可以生成swagger在线文档。通过http请求,从而实现服务与集群外部进行通信,当http请求到达webhost主机后,silky中间件通过webapi和请求方法路由到服务条目,然后通过内部实现的rpc通信与微服务应用进行通信。

**RPC的过滤器**: rpc通信支持两种类型的过滤器,在客户端发起请求过程中,会依次调用开发者定义的`IClientFilter`过滤器,服务端收到请求后,会依次调用`IServerFilter`然后再执行应用方法本身。

**RpcContext**: 可以通过`RpcContext.Context`添加或是获取本次rpc调用的`Attachments`参数。当然,开发者也可以通过注入`IRpcContextAccessor`获取本次通信的上线文参数`RpcContext`。

**获取当前登录用户**: 开发者可以通过`NullSession.Instance`获取当前登录用户,如果您已经登录系统,那么通过该接口可以获取到当前登录用户的`userId`、`userName`等信息。


## 服务治理

针对每个服务条目(应用服务接口方法),都实现了服务治理,开发者可以通过`governance`或是`[Governance()]`特性对服务的最大并发量、负载均衡算法、执行超时时间、是否使用缓存拦截、失败回调接口、接口是否对外网屏蔽等等属性进行配置。

以下描述了以服务条目为治理单位的属性表单：

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| AddressSelectorMode | 负载均衡策略 | Polling(轮询) | 负载均衡算法支持：Polling(轮询)、Random(随机)、HashAlgorithm(哈希一致性，根据rpc参数的第一个参数值) |
| ExecutionTimeout | 执行超时时间 | 3000(ms) | 单位为(ms),超时时发生熔断，-1表示在rpc通信过程中不会超时 |
| CacheEnabled | 是否启用缓存拦截 | true | rpc通信中是否启用缓存拦截 |
| MaxConcurrent | 允许的最大并发量 | 100 |  |
| FuseProtection | 是否开启熔断保护  | true |  |
| FuseSleepDuration | 熔断休眠时长  | 60(s) | 发生熔断后,多少时长后再次重试该服务实例 |
| FuseTimes | 服务提供者允许的熔断次数  | 3 | 服务实例连续n次发生熔断端,服务实例将被标识为不健康 |
| FailoverCount | 故障转移次数  | 0 | rpc通信异常情况下,允许的重新路由服务实例的次数,0表示有几个服务实例就转移几次 |
| ProhibitExtranet | 是否禁止外网访问  | false | 该属性只允许通过`GovernanceAttribute`特性进行设置 |
| FallBackType | 失败回调指定的类型  | null | 类型为`Type`,如果指定了失败回调类型,那么在服务执行失败,则会执行该类型的`Invoke`方法,该类型,必须要继承`IFallbackInvoker`该接口 |

开发者还可以通过`[Governance()]`特性对某个服务方法进行标识,被该特性标识的治理属性将覆盖微服务的配置/缺省值。

## 缓存拦截

为提高应用的响应,silky支持缓存拦截。缓存拦截在应用服务接口方法上通过缓存拦截特性进行设置,在silky框架中,存在如下三中类型的缓存特性，分别对数据缓存进行新增、更新、删除。

1. 设置缓存特性--`GetCachingInterceptAttribute`

2. 更新缓存特性--`UpdateCachingInterceptAttribute`

3. 删除缓存特性--`RemoveCachingInterceptAttribute`

使用缓存拦截,必须要保证缓存一致性。在rpc通信过程中,使用缓存拦截,同一数据的缓存依据可能会不同(设置的`KeyTemplate`,例如:缓存依据可能会根据`Id`、`Name`、`Code`分别进行缓存),从而产生不同的缓存数据,但是在对数据进行更新、删除操作时,由于无法通过`RemoveCachingInterceptAttribute`特性一次性删除该类型数据的所有缓存数据,这个时候，在实现业务代码过程中,就需要通过分布式缓存接口`IDistributedCache<T>`实现缓存数据的更新、删除操作。

## 服务注册中心

silky使用zookeeper作为默认服务的注册中心。当前还未扩展支持其他的服务注册中心。

silky支持为微服务集群配置多个服务注册中心，您只需要在配置服务注册中心的链接字符串`registrycenter:connectionStrings`中,使用分号`;`就可以指定微服务框架的多个服务注册中心。

为微服务配置服务注册中心如下所示:

```shell
registrycenter: // 服务注册中心配置节点
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 // 服务配置中心链接
  registryCenterType: Zookeeper // 注册中心类型
  connectionTimeout: 1000 // 链接超时时间(单位:ms)
  sessionTimeout: 2000 // 会话超时时间(单位:ms)
  operatingTimeout: 4000 // 操作超时时间(单位:ms)
  routePath: /services/serviceroutes
```

## 模块化管理

silky框架存在两种类型的模块:

1. 开发者通过继承`SilkyModule`就可以定义一个普通模块类;
2. 也可以通过继承`StartUpModule`定义一个服务注册启动模块类;开发者也可以通过继承`StartUpModule`,选择合适的依赖包,实现自己的代理主机。

**模块的依赖关系:** silky框架的模块通过`DependsOn`特性指定模块的依赖关系,silky框架支持通过直接或是间接的依赖模块。

## 依赖注入(服务注册与解析)

1. 通过继承依赖注入标识接口实现服务的注册**(推荐)**
silky框架提供了三个依赖注册的相关标识接口：`ISingletonDependency`(单例模式)、`IScopedDependency`(区域模式)、`ITransientDependency`(瞬态模式)。在微服务应用启动时,会扫描继承了这些标识接口的类(服务),并将其自身和继承的接口注册到Ioc容器中。

2. 定义模块,并在模块中通过`RegisterServices()`方法的`ContainerBuilder`注册服务(autofac),或是通过`ConfigureServices()`方法的`IServiceCollection`注册服务(微软官方自带的ioc容器)

3. 通过继承`IConfigureService`或是`ISilkyStartup`,通过`Configure()`方法的`IServiceCollection`注册服务

Silky因为支持通过`IServiceCollection`进行服务注册,所以可以很方便的与第三方服务(组件)进行整合,例如:`CAP`或是`MassTransit`等分布式事件框架。


## 使用Serilog作为日志记录器

silky框架提供了`serilog`作为日志记录器。只需要在构建主机时,增加`UseSerilogDefault()`,并添加`Serilog`相关配置即可。

代码:

```csharp
public static async Task Main(string[] args)
{
    await CreateHostBuilder(args).Build().RunAsync();
}

private static IHostBuilder CreateHostBuilder(string[] args)
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
        .RegisterSilkyServices<OrderHostModule>()
        .UseSerilogDefault();
    return hostBuilder;
}
```

配置:

```yml
serilog:
  minimumLevel:
    default: Information
    override:
      Microsoft: Warning
      Microsoft.Hosting.Lifetime: Information
      Silky: Debug
  writeTo:
    - name: File
      args:
        path: "./logs/log-.log"
        rollingInterval: Day
    - name: Console
      args:
        outputTemplate: "[{Timestamp:yyyy/MM/dd HH:mm:ss} {Level:u11}] {Message:lj} {NewLine}{Exception}{NewLine}"
        theme: "Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme::Literate, Serilog.Sinks.Console"
```

## 使用Miniprofile对http请求进行性能监控

要求必须在web主机项目(一般为网关项目)安装`Silky.Http.MiniProfiler`包,并将`swaggerDocument:injectMiniProfiler`配置项的属性设置为`true`。

```yml
swaggerDocument:
  injectMiniProfiler: true
```
![silky-ms2.png](/assets/imgs/silky-ms2.png)


## 使用skywalking查看链路跟踪

要求微服务在启动模块依赖`SilkySkyApmAgentModule`模块,并配置`skyWalking`相关属性。

```csharp
 [DependsOn(typeof(SilkySkyApmAgentModule))]
public class AccountHostModule : GeneralHostModule
{
}
```

```yaml
skyWalking:
  serviceName: AccountHost
  headerVersions:
    - sw8
  sampling:
    samplePer3Secs: -1
    percentage: -1.0
  logging:
    level: Debug
    filePath: "logs/skyapm-{Date}.log"
  transport:
    interval: 3000
    protocolVersion: v8
    queueSize: 30000
    batchSize: 3000
    gRPC:
      servers: "127.0.0.1:11800"
      timeout: 10000
      connectTimeout: 10000
      reportTimeout: 600000
```
在silky的实例项目中,提供了`skyWalking`通过`docker-compose`快速启动的服务编排文件`samples\docker-compose\infrastr\docker-compose.skywalking.yml`,开发者只需要进入到`samples\docker-compose\infrastr`目录中,通过如下命令即可开始的启动一个skyWalking服务。

```shell
cd samples\docker-compose\infrastr
docker-compose -f docker-compose.skywalking.yml
```

打开`http://127.0.0.1:8180`即可看到微服务集群的运行情况：

网络拓扑图:

![silky-ms3.png](/assets/imgs/silky-ms3.png)


链路跟踪:

![silky-ms4.png](/assets/imgs/silky-ms4.png)

仪表盘:

![silky-ms5.png](/assets/imgs/silky-ms5.png)

## 使用Apollo作为服务配置中心

### 部署Apollo服务

必要前提是开发者已经部署了一套Apollo服务。开发者可以参考[Apollo部署节点](https://www.apolloconfig.com/#/zh/deployment/distributed-deployment-guide),部署一套Apollo服务。

在开发过程中,更简单的做法是使用silky实例项目中使用docker-compose已经编排好的文件` docker-compose.apollo.yml`。

进入到`samples\docker-compose\infrastr`目录,将`.env`设置的环境变量`EUREKA_INSTANCE_IP_ADDRESS`修改为您**当前本机的IP地址,不允许为`127.0.0.1`**。

运行如下命令,等待1~2分钟即可启动apollo配置服务。

```powershell
docker-compose -f docker-compose.apollo.yml up -d
```

### 使用Apollo作为微服务的配置中心

1. 在主机项目通过nuget安装`Silky.Apollo`包。(这是一个空包,您也可以直接安装`Com.Ctrip.Framework.Apollo.AspNetCoreHosting`和`Com.Ctrip.Framework.Apollo.Configuration`包)

2. 在服务注册时,添加对Appo服务配置中心的支持

```csharp
private static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .RegisterSilkyServices<AccountHostModule>()
        .AddApollo();
}
```

如果您您想在指定的运行环境中使用Apollo作为微服务的配置中心，而在另外其他运行环境中使用本地配置,那么您也可以通过如下当时处理：

```csharp
private static IHostBuilder CreateHostBuilder(string[] args)
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
        .RegisterSilkyServices<AccountHostModule>()
        .UseSerilogDefault();
    if (EngineContext.Current.IsEnvironment("Apollo"))
    {
        hostBuilder.AddApollo();
    }

    return hostBuilder;
}
```

> 备注 
> 运行环境您可以通过修改`Properties\launchSettings.json`的`DOTNET_ENVIRONMENT`变量(本地开发模式)
> 或是通过`.env`环境变量文件指定`DOTNET_ENVIRONMENT`变量(docker-compose开发模式)

3. 在Apollo服务配置中心新建相关的应用,并新增相关的配置

打开地址:http://127.0.0.1:8070 (Applo服务的web管理工具:portal),新建相关的应用。如下图:

![silky-ms6.png](/assets/imgs/silky-ms6.png)

为应用添加相应的配置:

![silky-ms7.png](/assets/imgs/silky-ms7.png)

普通业务微服务的配置如下:

```properties
# Application
rpc:port = 2201
connectionStrings:default = server=127.0.0.1;port=3306;database=order;uid=root;pwd=qwe!P4ss;
jwtSettings:secret = jv1PZkwjLVCEygM7faLLvEhDGWmFqRUW

# TEST1.silky.sample
registrycenter:registryCenterType = Zookeeper
registrycenter:connectionStrings = 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186

distributedCache:redis:isEnabled = true
distributedCache:redis:configuration = 127.0.0.1:6379,defaultDatabase=0

rpc:token = ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW

governance:executionTimeout = -1

cap:rabbitmq:hostName = 127.0.0.1
cap:rabbitmq:userName = rabbitmq
cap:rabbitmq:password = rabbitmq
```
web网关的配置如下:
```properties

# TEST1.silky.sample.gateway
gateway:injectMiniProfiler = true
gateway:enableSwaggerDoc = true
gateway:wrapResult = true
gateway:jwtSecret = jaoaNPA1fo1rcPfK23iNufsQKkhTy8eh
swaggerDocument:organizationMode = Group
swaggerDocument:injectMiniProfiler = true
swaggerDocument:termsOfService = https://www.github.com/liuhll/silky

# TEST1.silky.sample
registrycenter:registryCenterType = Zookeeper
registrycenter:connectionStrings = 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186

distributedCache:redis:isEnabled = true
distributedCache:redis:configuration = 127.0.0.1:6379,defaultDatabase=0

rpc:token = ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW

governance:executionTimeout = -1
```

4. 增加Apollo配置中心相关配置(默认读取`appsettings.yml`),如果指定运行环境变量则读取`appsettings.{Environment}.yml`中的配置

例如:

```yml
apollo:
  appId: "silky-stock-host"
  cluster: default
  metaServer: "http://127.0.0.1:8080/"
  #  secret: "ffd9d01130ee4329875ac3441c0bedda"
  namespaces:
    - application
    - TEST1.silky.sample
  env: DEV
  meta:
    DEV: "http://127.0.0.1:8080/"
    PRO: "http://127.0.0.1:8080/"
```


## 分布式锁

silky使用[DistributedLock](https://github.com/madelson/DistributedLock)作为分布式锁,在服务路由注册和分布式事务作业中均使用了分布式锁.

## 身份认证与授权

silky身份认证与授权通过包`Silky.Http.Identity`，通过webhost在网关实现统一的身份认证和授权。

### 用户登陆与签发token

silky通过`Silky.Jwt`包提供的`IJwtTokenGenerator`实现jwt格式的token。签发token的微服务应用需要通过nuget安装`Silky.Jwt`包，并在启动模块中依赖`JwtModule`模块。开发者可以对签发的token的密钥、token有效期、Jwt签名算法、签发者、受众等属性通过配置节点`jwtSettings`进行配置。开发者至少需要对`jwtSettings:secret`进行配置。

配置如下:
```yaml
jwtSettings:
  secret: jv1PZkwjLVCEygM7faLLvEhDGWmFqRUW
```

用户登陆接口如下:

```csharp
        public async Task<string> Login(LoginInput input)
        {
            var userInfo = await _accountRepository.FirstOrDefaultAsync(p => p.UserName == input.Account
                                                                             || p.Email == input.Account);
            if (userInfo == null)
            {
                throw new AuthenticationException($"不存在账号为{input.Account}的用户");
            }

            if (!userInfo.Password.Equals(_passwordHelper.EncryptPassword(userInfo.UserName, input.Password)))
            {
                throw new AuthenticationException("密码不正确");
            }

            var payload = new Dictionary<string, object>()
            {
                { ClaimTypes.UserId, userInfo.Id },
                { ClaimTypes.UserName, userInfo.UserName },
                { ClaimTypes.Email, userInfo.Email },
            };
            return _jwtTokenGenerator.Generate(payload);
        }
```

### 身份认证

1. 网关项目(WebHost)的启动模块需要依赖`IdentityModule`模块

```csharp
    [DependsOn(typeof(IdentityModule))]
    public class GatewayHostModule : WebHostModule
    {
        
    }
```

2. `gateway:jwtSecret`配置的属性必须与签发token的微服务应用配置的属性`jwtSettings:secret`的值保持一致。

```yaml
gateway:
  jwtSecret: jv1PZkwjLVCEygM7faLLvEhDGWmFqRUW
```

3. 匿名访问

开发者只需要在应用接口或是应用接口方法中标注`[AllowAnonymous]`特性即可，这样无需用户登陆,也可以访问该接口。

```csharp
[AllowAnonymous]
Task<string> Login(LoginInput input);
```

### 授权

开发者可以在网关应用通过继承`SilkyAuthorizationHandler`基类,并重写`PipelineAsync`方法即可实现对自定义授权。

```csharp
 public class TestAuthorizationHandler : SilkyAuthorizationHandler
    {
        private readonly ILogger<TestAuthorizationHandler> _logger;
        private readonly IAuthorizationAppService _authorizationAppService;

        public TestAuthorizationHandler(ILogger<TestAuthorizationHandler> logger,
        IAuthorizationAppService authorizationAppService)
        {
            _logger = logger;
           _authorizationAppService = authorizationAppService;
        }

        public async override Task<bool> PipelineAsync(AuthorizationHandlerContext context,
            DefaultHttpContext httpContext)
        {
            // 获取访问的服务条目
            var serviceEntry = httpContext.GetServiceEntry();
           
            // 可以通过rpc调用IdentifyApp,实现自定义的授权 
           return _authorizationAppService.Authorization(sserviceEntry.ServiceDescriptor.Id);
           
        }
    }
```

## 对象属性映射
silky实现了基于[AutoMapper](https://github.com/AutoMapper/AutoMapper)和[Mapster](https://github.com/MapsterMapper/Mapster)的对象属性映射的包。实现的代理主机默认依赖`MapsterModule`包,使用Mapster作为对象映射的组件。

只需要通过`Adapt`方法即可实现对象属性映射。


## 使用efcore作为数据访问组件

efcore数据访问组件主要参考了[furion](https://dotnetchina.gitee.io/furion/docs/dbcontext-start)的实现。提供了数据仓库、数据库定位器、多租户等实现方式。使用方式与其基本保持一致。