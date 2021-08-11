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

## 服务治理

开发者可以通过`governance`或是` [Governance()]`特性对服务的最大并发量、负载均衡算法、执行超时时间、是否使用缓存拦截、失败回调接口、接口是否对外网屏蔽等等属性进行配置。

## 缓存拦截

## 服务注册中心

## 模块化管理

## 服务注册与解析

## 使用skywalking查看链路跟踪

## 使用Apollo作为服务配置中心

## 分布式锁

## 身份认证与授权

## 使用ef作为数据访问组件