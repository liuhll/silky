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

        public void AddTcpServices()
        {
            var rpcEndpoint = RpcEndpointHelper.GetLocalTcpEndpoint();
            _server.Endpoints.Add(rpcEndpoint);
            var tcpServices = _serviceManager.GetLocalService(ServiceProtocol.Tcp);
            foreach (var tcpService in tcpServices)
            {
                _server.Services.Add(tcpService.ServiceDescriptor);
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

3. 主机服务提供者`DefaultServerProvider`提供三个核心的方法`AddTcpServices()`、`AddHttpServices()`、`AddWsServices()`; 在应用启动时,在指定的时刻查找指定协议的服务和相应的服务终结点;
  
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