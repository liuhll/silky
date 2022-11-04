---
title: Silky服务主机
lang: zh-cn
---

## Silky服务主机的概念

在Silky微服务框架中,[主机](host.html)用于托管微服务应用,在微服务主机启动时,最重要的任务就是构建服务提供者,并将服务提供者主机的信息以元数据的形式注册到 **服务注册**,集群中的每个微服务应用可以通过 **心跳** 或是 **订阅** 的方式从服务注册中心获取整个微服务集群最新的元数据信息。

在Silky框架中,我们通过 [Server](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Rpc/Runtime/Server/Server.cs) 来定义Silky主机(也就是服务提供者)。一个微服务提供者的主要属性如下表所示:

| 属性名 | 名称  | 备注           |
|:-------|:------|:--------------|
| HostName | 微服务主机名称  |  等于应用程序的启动程序集的名称    |
| Endpoints | 微服务提供者所有的终结点  | 在构建支持不同协议的服务时,动态的添加该服务主机的终结点   |
| Services | 微服务提供者所有的服务描述符  |    |

在服务注册过程中,并不能直接注册`Server`的信息,所以我们定义了 **服务主机描述符** ,通过`服务主机描述符`[ServerDescriptor](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Rpc/Runtime/Server/ServerDescriptor.cs) 来描述Silky主机信息,并通过其将主机的信息注册到服务注册中心,服务主机`Server`与服务主机描述符`ServerDescriptor`可以相互转化。

## 创建Silky服务主机

Silky服务主机的解析由默认主机服务提供者`DefaultServerProvider`负责创建和维护;

```csharp
    public class DefaultServerProvider : IServerProvider
    {
        public ILogger<DefaultServerProvider> Logger { get; set; }
        private readonly IServer _server;
        private readonly IServiceManager _serviceManager;
        private readonly ISerializer _serializer;

        public DefaultServerProvider(IServiceManager serviceManager,
            ISerializer serializer)
        {
            _serviceManager = serviceManager;
            _serializer = serializer;
            Logger = EngineContext.Current.Resolve<ILogger<DefaultServerProvider>>();
            _server = new Server(EngineContext.Current.HostName);
        }

        public void AddRpcServices()
        {
            var rpcEndpoint = EndpointHelper.GetLocalRpcEndpoint();
            _server.Endpoints.Add(rpcEndpoint);
            var rpcServices = _serviceManager.GetLocalService(ServiceProtocol.Rpc);
            foreach (var rpcService in rpcServices)
            {
                _server.Services.Add(rpcService.ServiceDescriptor);
            }
        }

        public void AddHttpServices()
        {
            var webEndpoint = RpcEndpointHelper.GetLocalWebEndpoint();
            if (webEndpoint == null)
            {
                throw new SilkyException("Failed to obtain http service rpcEndpoint");
            }

            _server.Endpoints.Add(webEndpoint);
        }

        public void AddWsServices()
        {
            var wsEndpoint = RpcEndpointHelper.GetWsEndpoint();
            _server.Endpoints.Add(wsEndpoint);
            var wsServices = _serviceManager.GetLocalService(ServiceProtocol.Ws);
            foreach (var wsService in wsServices)
            {
                _server.Services.Add(wsService.ServiceDescriptor);
            }
        }

        public IServer GetServer()
        {
            Logger.LogDebug("server endpoints:" + _serializer.Serialize(_server.Endpoints.Select(p => p.ToString())));
            if (_server.HasHttpProtocolServiceEntry() && !_server.Endpoints.Any(p =>
                    p.ServiceProtocol == ServiceProtocol.Http || p.ServiceProtocol == ServiceProtocol.Https))
            {
                throw new SilkyException(
                    "A server that supports file upload and download or ActionResult must be built through the http protocol host",
                    StatusCode.ServerError);
            }
            return _server;
        }
    }
```

从上面的代码我们可以看出:

1. 由Server主机提供者的构造器创建`Server`主机;

2. Server主机提供者的构造器中注入服务管理器`IServiceManager`,由此,我们也可以得知:在应用启动时获取主机提供者的时候,实现了[服务和服务条目的解析](service-serviceentry.html);

3. 主机服务提供者`DefaultServerProvider`提供三个核心的方法`AddRpcServices()`、`AddHttpServices()`、`AddWsServices()`; 在应用启动时,在指定的时刻查找指定协议的服务和相应的服务终结点;
  
  3.1 由[web主机](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0)创建的Silky微服务应用,映射Silky路由的时候,调用`AddHttpServices()`方法,在应用启动成功时,添加该微服务应用的Http终结点;
  
  ```csharp
  public static class SilkyEndpointRouteBuilderExtensions
  {
        public static ServiceEntryEndpointConventionBuilder MapSilkyRpcServices(this IEndpointRouteBuilder endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var hostApplicationLifetime = endpoints.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
            // 在应用启动后注册RegisterSilkyWebServer()方法
            hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                // 注册支持Web服务的Silky微服务
                await RegisterSilkyWebServer(endpoints.ServiceProvider);
            });
            return GetOrCreateServiceEntryDataSource(endpoints).DefaultBuilder;
        }

        private static async Task RegisterSilkyWebServer(IServiceProvider serviceProvider)
        {
            // 获取主机服务提供者实例
            var serverRegisterProvider =
                serviceProvider.GetRequiredService<IServerProvider>();

            serverRegisterProvider.AddHttpServices();
        }      
  }
  ``` 

```csharp

public class DefaultServerProvider : IServerProvider
{
    public void AddHttpServices()
    {
        // 获取 http 终结点
        var webEndpoint = RpcEndpointHelper.GetLocalWebEndpoint();
        if (webEndpoint == null) // 获取失败则抛出异常
        {
            throw new SilkyException("Failed to obtain http service rpcEndpoint");
        }
        // 将http 终结点添加的服务提供者的终结点列表中
        _server.Endpoints.Add(webEndpoint);
    }

   // 其他代码略...
}

```

  从上述代码我们可以看到,只有使用web主机构建(托管)应用的主机，在服务启动过程中才会有将http终结点添加到silky服务提供者的终结点列表中；Silky服务内部之间的通信是由dotnetty实现rpc框架,http终结点的用途是提供了对服务外部访问的入口;

::: tip 备注

如果是由[Web主机](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0) 托管的Silky应用,那么在在此时才会首次获取`DefaultServerProvider`的实例,也就是在此时才会进行服务与服务条目的解析;
:::

  3.2  在模块`DotNettyTcpModule`初始化任务的过程中,从Ioc容器中获取到消息监听者`DotNettyTcpServerMessageListener`实例后,完成监听任务后，添加支持RPC
  协议的服务;
  
  ```csharp
   [DependsOn(typeof(RpcModule), typeof(DotNettyModule))]
    public class DotNettyTcpModule : SilkyModule
    {
        // 其他代码略...
    
        public override async Task Initialize(ApplicationContext applicationContext)
        {
            //获取消息监听者实例
            var messageListener =
                applicationContext.ServiceProvider.GetRequiredService<DotNettyTcpServerMessageListener>();
            // 完成消息监听任务
            await messageListener.Listen();
            // 获取silky主机服务提供者实例
            var serverProvider =
                applicationContext.ServiceProvider.GetRequiredService<IServerProvider>();
            // 添加支持TCP协议的服务
            serverProvider.AddRpcServices();
        }
    }
  ```

  通过上面的代码我们看到,只有在完成服务端消息监听任务之后,Silky服务主机才会完成添加支持RPC协议的服务,支持RPC的服务就是前文所述的[应用服务](service-serviceentry.html#应用服务的解析);Silky微服务之间的通信主要是由dotnetty实现的RPC框架完成的。
  
  ```csharp
  public class DefaultServerProvider : IServerProvider
  {
    public void AddRpcServices()
    {
        var rpcEndpoint = EndpointHelper.GetLocalRpcEndpoint();
        _server.Endpoints.Add(rpcEndpoint);
        var rpcServices = _serviceManager.GetLocalService(ServiceProtocol.Rpc);
        foreach (var rpcService in rpcServices)
        {
            _server.Services.Add(rpcService.ServiceDescriptor);
        }
    }

   // 其他代码略...
  }
  ```

::: tip 备注
如果是由[通用主机](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-6.0) 托管的Silky应用,那么在在此时才会首次获取`DefaultServerProvider`的实例,也就是在此时才会进行服务与服务条目的解析;
:::

   3.3  在模块`WebSocketModule`初始化任务的过程中,查找到所有支持`WebSocket`的服务,并通过`WebSocketServerBootstrap`的实例完成创建ws服务，这些服务将会提供`WebSocket`服务,任务完成后,将通过Silky主机服务提供者`DefaultServerProvider`的实例添加对`ws服务`;
 
   ```csharp
    [DependsOn(typeof(RpcModule))]
    public class WebSocketModule : SilkyModule
    {

        // 其他代码略...

        public override async Task Initialize(ApplicationContext applicationContext)
        {
            var typeFinder = applicationContext.ServiceProvider.GetRequiredService<ITypeFinder>();
            var webSocketServices = GetWebSocketServices(typeFinder);
            var webSocketServerBootstrap =
                applicationContext.ServiceProvider.GetRequiredService<WebSocketServerBootstrap>();
            webSocketServerBootstrap.Initialize(webSocketServices);
            var serverProvider =
                applicationContext.ServiceProvider.GetRequiredService<IServerProvider>();

            serverProvider.AddWsServices();
        }

        private (Type, string)[] GetWebSocketServices(ITypeFinder typeFinder)
        {
            var wsServicesTypes = ServiceHelper.FindServiceLocalWsTypes(typeFinder);
            return wsServicesTypes.Select(p => (p, WebSocketResolverHelper.ParseWsPath(p))).ToArray();
        }
    }
   ```

```csharp
public class DefaultServerProvider : IServerProvider
{
    public void AddWsServices()
    {
        var wsEndpoint = RpcEndpointHelper.GetWsEndpoint();
        _server.Endpoints.Add(wsEndpoint);
        var wsServices = _serviceManager.GetLocalService(ServiceProtocol.Ws);
        foreach (var wsService in wsServices)
        {
            _server.Services.Add(wsService.ServiceDescriptor);
        }
    }

 // 其他代码略...
}
```

websocket服务是如何解析,如何创建支持websocket的服务这个我们将会在之后的文档中介绍;

::: tip 备注

1. 只有依赖了`WebSocketModule`模块的Silky应用,才支持提供`WebSocket`服务,提供`WebSocket`服务必须要求继承`Silky.WebSocket.WsAppServiceBase`；

2. silky框架的websocket是通过网关实现代理的,通过代理再与具体的Silky应用服务提供者进行连接;

3. websocket服务是由框架[websocket-sharp-core](https://github.com/ImoutoChan/websocket-sharp-core)提供的；

4. websocket服务提供的方法也会被解析为服务条目,也可以与其他微服务实例实现RPC通信;
:::