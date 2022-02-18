---
title: 服务治理
lang: zh-cn
---

## 服务治理的概念

服务治理是主要针对分布式服务框架、微服务，处理服务调用之间的关系，服务发布和发现（谁是提供者，谁是消费者，要注册到哪里），出了故障谁调用谁，服务的参数都有哪些约束,如何保证服务的质量？如何服务降级和熔断？怎么让服务受到监控，提高机器的利用率?

微服务有哪些问题需要治理？

1. **服务注册与发现**: 单体服务拆分为微服务后，如果微服务之间存在调用依赖，就需要得到目标服务的服务地址，也就是微服务治理的 **服务发现** 。要完成服务发现，就需要将服务信息存储到某个载体，载体本身即是微服务治理的*服务注册中心*，而存储到载体的动作即是*服务注册*。

2. **可观测性**: 微服务由于较单体应用有了更多的部署载体，需要对众多服务间的调用关系、状态有清晰的掌控。可观测性就包括了调用拓扑关系、监控（Metrics）、日志（Logging）、调用追踪（Trace）等。

3. **流量管理**: 由于微服务本身存在不同版本，在版本更迭过程中，需要对微服务间调用进行控制，以完成微服务版本更迭的平滑。这一过程中需要根据流量的特征（访问参数等）、百分比向不同版本服务分发，这也孵化出灰度发布、蓝绿发布、A/B测试等服务治理的细分主题。

4. **服务容错**: 任何服务都不能保证100%不出问题，生产环境复杂多变，服务运行过程中不可避免的发生各种故障（宕机、过载等等），工程师能够做的是在故障发生时尽可能降低影响范围、尽快恢复正常服务,需要引入「熔断、隔离、限流和降级、超时机制」等「服务容错」机制来保证服务持续可用性。

4. **安全**: 不同微服务承载自身独有的业务职责，对于业务敏感的微服务，需要对其他服务的访问进行认证与鉴权，也就是安全问题。

5. **控制**： 对服务治理能力充分建设后，就需要有足够的控制能力，能实时进行服务治理策略向微服务分发。

6. **服务本身的治理**: 确保微服务主机的健康,有能力将不健康节点从微服务集群中移除。

## 服务注册与发现

silky支持服务的自动注册和发现,支持使用 **Zookeeper** 、**Nacos** 、**Consul** 作为服务注册中心。服务实例上线、下线智能感知。

1. 当服务实例启动时,会向服务注册中心新增或是更新服务元数据(*如果不存在新增服务元数据、如果存在服务元数据则更新*);同时,更新服务注册中心该实例的终结点(实例地址信息)。

2. 使用 **Zookeeper** 或是 **Nacos** 作为服务注册中心,会通过 **发布-订阅** 的方式从服务注册中心获取最新的服务元数据和服务实例的终结点(实例地址)信息,并更新到本地内存;

3. 如果使用 **Consul** 作为服务注册中心，则会通过心跳的方式从服务注册中心 **拉取** 最新的服务元数据和服务实例的终结点(实例地址)信息。当服务注册中心的终结点(地址信息)发生变化,服务实例的内存中服务路由表信息也将得到更新。

4. 当在RPC通信过程中发生IO异常或是通信异常时,服务实例将会在n(配置属性为:`Governance:UnHealthAddressTimesAllowedBeforeRemoving`)次后从服务注册中心移除。(`UnHealthAddressTimesAllowedBeforeRemoving`如果的值等于0,则服务实例将会被立即移除)。

5. 在RPC通信过程中,采用长链接, 支持心跳检测。在服务之间建立连接后,如果`Governance:EnableHeartbeat`配置为`true`，那么会定时(通过配置`Governance:HeartbeatWatchIntervalSeconds`)发送一个心跳包,从而保证会话链接的可靠性。如果心跳检测到通信异常,则会根据配置属性(`Governance:UnHealthAddressTimesAllowedBeforeRemoving`)n次后,从服务注册中心移除。

## 负载均衡

在RPC通信过程中,silky框架支持 **轮询(Polling)**、 **随机(Random)** 、 **哈希一致性(HashAlgorithm)** 等负载均衡算法。负载均衡的缺省值为 **轮询(Polling)** ,开发者可以通过配置属性 `Governance:ShuntStrategy` 来统一指定负载均衡算法。同时,开发者也可以通过`GovernanceAttribute`特性来重置应用服务方法(服务条目)的负载均衡算法。

例如:

```csharp
[HttpGet("{name}")]
[Governance(ShuntStrategy = ShuntStrategy.HashAlgorithm)]
Task<TestOut> Get([HashKey]string name);
```

如果选择使用 **哈希一致性(HashAlgorithm)** 作为负载均衡算法,则需要使用`[HashKey]`对某一个参数进行标识,这样,相同参数的请求,在RPC通信过程中,都会被路由到通一个服务实例。

## 超时

在RPC通信中,如果在给定的配置时长没有返回结果,则会抛出超时异常。一般地,开发者可以通过配置属性`Governance:TimeoutMillSeconds`来统一的配置RPC调用超时时长,缺省值为`5000`ms。同样地,开发者也可以通过`GovernanceAttribute`特性来重置应用服务方法的超时时长。


```csharp
[HttpGet("{name}")]
[Governance(TimeoutMillSeconds = 1000)]
Task<TestOut> Get([HashKey]string name);
```


如果将超时时长配置为`0`，则表示在RPC调用过程中,不会出现超时异常,直到RPC调用返回结果或是抛出RPC调用抛出其他异常。

::: tip 提示

建议在开发环境中, 将配置属性`Governance:TimeoutMillSeconds`设置为`0`,方便开发者进行调试。

:::

## 故障转移(失败重试)

在RPC通信过程中,如果发生IO异常(`IOException`)、通信异常(`CommunicationException`)、或是找不到本地服务条目(服务提供者抛出`NotFindLocalServiceEntryException`异常)、超出服务提供者允许的最大处理并发量(`NotFindLocalServiceEntryException`),则服务消费者会根据配置的次数选择其他服务实例重新调用。

1. 如果RPC调用过程中发生的是IO异常(`IOException`)或是通信异常(`CommunicationException`)或是服务提供者抛出`NotFindLocalServiceEntryException`异常，将会把选择的服务实例的状态变更为不可用状态,在`Governance:UnHealthAddressTimesAllowedBeforeRemoving`次标识后,服务实例将会下线(*将服务提供者的实例地址从服务注册中心移除*)。

2. 如果超出服务提供者实例允许的最大并发量,则会选择其他服务实例进行调用,但不会变更服务实例的状态。(换句话说,也就是服务提供者触发了限流保护)

3. 其他类型的异常不会导致失败重试。

开发者通过`Governance:RetryTimes`配置项来确定失败重试的次数，缺省值等于`3`。同样地,开发者也可以通过`GovernanceAttribute`特性来重置失败重试次数。如果`RetryTimes`被设置为小于等于`0`,则不会发生失败重试。通过`Governance:RetryIntervalMillSeconds` 可以配置失败重试的间隔时间。

```csharp
[HttpGet("{name}")]
[Governance(RetryTimes = 3)]
Task<TestOut> Get([HashKey]string name);
```


开发者也可以自定义失败重试策略,只需要继承`InvokeFailoverPolicyProviderBase`基类,通过重写`Create`方法构建失败策略。

下面的例子演示了定义超时重试的策略:

```csharp
public class TimeoutFailoverPolicyProvider : InvokeFailoverPolicyProviderBase
{
       
    private readonly IServerManager _serverManager;

    public TimeoutFailoverPolicyProvider( IServerManager serverManager)  
    {
        _serverManager = serverManager;
    }

    public override IAsyncPolicy<object> Create(string serviceEntryId, object[] parameters)
    {
        IAsyncPolicy<object> policy = null;
        var serviceEntryDescriptor = _serverManager.GetServiceEntryDescriptor(serviceEntryId);

        if (serviceEntryDescriptor?.GovernanceOptions.RetryTimes > 0) 
        {
            policy = Policy<object>
                        .Handle<Timeoutxception>()
                        .Or<SilkyException>(ex => ex.GetExceptionStatusCode() == StatusCode.Timeout)
                        .WaitAndRetryAsync(serviceEntryDescriptor.GovernanceOptions.RetryTimes,
                            retryAttempt =>
                                TimeSpan.FromMilliseconds(serviceEntryDescriptor.GovernanceOptions.RetryIntervalMillSeconds),
                            (outcome, timeSpan, retryNumber, context)
                                => OnRetry(retryNumber, outcome, context));
        }
        return policy;
    }
}

```

## 熔断保护(断路器)

RPC通信过程中,在开启熔断保护的情况下,如果在连续发生n次 **非业务类异常** (*非业务类异常包括友好类异常、鉴权类异常、参数校验类异常等*)，则会触发熔断保护,在一段时间内,该服务条目将不可用。

开发者通过`Governance:EnableCircuitBreaker`配置项来确定是否要开启熔断保护，通过`Governance:ExceptionsAllowedBeforeBreaking`配置项来确定在熔断保护触发前允许的异常次数(这里的异常为非业务类异常)，通过`Governance:BreakerSeconds`配置项来确定熔断的时长(单位为:秒)。同样地,熔断保护也可以通过`GovernanceAttribute`来进行配置。


```csharp
[HttpGet("{name}")]
[Governance(EnableCircuitBreaker = true,ExceptionsAllowedBeforeBreaking = 2, BreakerSeconds = 120)]
Task<TestOut> Get([HashKey]string name);
```

## 限流

限流的目的是通过对并发访问/请求进行限速，或者对一个时间窗口内的请求进行限速来保护系统，一旦达到限制速率则可以拒绝服务、排队或等待、降级等处理。

Silky微服务框架的限流分为两部分,一部分是服务内部RPC之间的通信,一部分是对HTTP请求的进行限流。

### RPC限流

当服务提供者接收到RPC请求后,如果当前服务实例并发处理量大于配置的`Governance:MaxConcurrentHandlingCount`,当前实例无法处理该请求,会抛出`OverflowMaxServerHandleException`异常。服务消费者会根据配置重试该服务的其他实例，可参考[故障转移(失败重试)](#熔断保护-断路器)节点。

`Governance:MaxConcurrentHandlingCount`的配置缺省值为`50`,如果配置小于等于`0`，则表示不对rpc通信进行限流。这里的配置针对的是服务实例所有并发处理的能力,并不是针对某个服务条目的并发量配置,所以开发者无法通过`GovernanceAttribute`特性来修改并发处理量的配置。

### HTTP限流

Silky框架除了支持服务内部之间RPC调用的限流之外,还支持通过[AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit)实现对Http请求的限流。**AspNetCoreRateLimit** 支持通过Ip或是针对IP进行限流。

下面我们来简述如何使用 **AspNetCoreRateLimit** 达到对http请求的限流。

#### 1. 添加配置

网关应用新增限流配置文件 **ratelimit.json**。在`RateLimiting:Client`配置节点配置针对客户端的限流通用规则,通过`RateLimiting:Client:Policies`配置节点重写针对特定客户端的限流策略。开发者可以参考[ClientRateLimitMiddleware](https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/ClientRateLimitMiddleware)熟悉相关的配置属性。在`RateLimiting:Ip`配置节点配置针对IP的限流通用规则,通过`RateLimiting:Ip:Policies`配置节点重写针对特定IP的限流策略。开发者可以参考[IpRateLimitMiddleware](https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/IpRateLimitMiddleware)熟悉相关的配置属性。

如果网关采用分布式部署,可以通过`RateLimiting:RedisConfiguration`属性配置redis服务作为存储服务。

例如:

```json
{
  "RateLimiting": {
    "Client": {
      "EnableEndpointRateLimiting": false,
      "StackBlockedRequests": false,
      "ClientIdHeader": "X-ClientId",
      "HttpStatusCode": 429,
      "EndpointWhitelist": [
        "get:/api/license",
        "*:/api/status"
      ],
      "ClientWhitelist": [
        "dev-id-1",
        "dev-id-2"
      ],
      "GeneralRules": [
        {
          "Endpoint": "*",
          "Period": "1s",
          "Limit": 5
        }
      ],
      "QuotaExceededResponse": {
        "Content": "{{ \"data\":null,\"errorMessage\": \"Whoa! Calm down, cowboy! Quota exceeded. Maximum allowed: {0} per {1}. Please try again in {2} second(s).\",\"status\":\"514\",\"statusCode\":\"OverflowMaxRequest\" }}",
        "ContentType": "application/json",
        "StatusCode": 429
      },
      "Policies": {
        "ClientRules": [
        ]
      }
    }
  },
  "RedisConfiguration": "127.0.0.1:6379,defaultDatabase=1"
}
```
#### 2. 注册服务

在 `Startup` 启动类中, 添加 **AspNetCoreRateLimit** 的相关服务。

```csharp
public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var redisOptions = configuration.GetRateLimitRedisOptions();
    services.AddClientRateLimit(redisOptions);
    services.AddIpRateLimit(redisOptions);
}
```

当然,如果开发者是通过 `AddSilkyHttpServices()` 进行服务注册,在这个过程中,已经同时添加了 **AspNetCoreRateLimit** 的相关服务

#### 3.启用 **AspNetCoreRateLimit** 的 相关中间件,实现HTTP限流。

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env) 
{
   app.UseClientRateLimiting(); // 针对客户端进行限流
  // app.UseIpRateLimiting(); // 针对IP进行限流

}
```

## 服务回退(服务降级)

在RPC调用过程中,如果执行失败,我们通过调用`Fallback方法`,从而达到服务降级操作。

想要实现服务回退的处理，我们需要在定义服务方法的时候通过`FallbackAttribute`特性指定的要回退的接口类型。给定的接口需要定义一个与服务方法参数相同的方法。

`FallbackAttribute`需要指定回退接口接口的类型,方法名称(如果缺省,则回退接口定义的方法名称与服务条目方法一致)，如果定义了多个回退方法,还要给出权重配置。

| 属性 | 配置 | 备注 |
|:-----|:-----|:-----|
| Type | 回退接口类型 | 必须指定,一般与服务接口定义在一起 |
| MethodName | 指定的回退方法 | 如果不配置的话,则与服务条目方法一致,定义的方法的参数必须一致 |

```csharp
  [HttpPatch]
  [Fallback(typeof(IUpdatePartFallBack))]
  Task<string> UpdatePart(TestInput input);
```

`IUpdatePartFallBack`需要定义一个与`UpdatePart`参数相同的方法。

```csharp
public interface IUpdatePartFallBack
{
    Task<string> UpdatePart(TestInput input);
}
    
```


定义的回退接口和方法只有在被实现后,才会在RPC调用失败后执行定义的`Fallback方法`。服务回退的实现方式有两种方式,一种是在服务端实现回退,一种是在客户端实现。

### 在服务端实现回退方法

如果在服务端实现回退方法,当RPC在服务端执行业务方法失败,如果服务端有存在定义的回退接口的实现,那么会降级执行回退方法。

举一个比较适用的场景，例如: 在一个短信收发的业务场景中,如果使用阿里云作为服务提供商相对更便宜,但是如果欠费后我们希望能够平滑的切换到腾讯云提供商。在这样的业务场景下,如果我们默认选择使用阿里云作为服务提供商, 我们就可以通过在服务端实现一个定义的`Fallback方法`从而达到平滑的使用备用短信服务提供商的作用。

### 在消费端实现回退方法

当然, 我们也可以在调用端(消费端)实现定义的`Fallback方法`，如果RPC调用失败,那么在调用端就会执行实现了的`Fallback方法`。返回给前端的是降级后的数据,并不会发生异常。

```csharp
public class TestUpdatePartFallBack : IUpdatePartFallBack, IScopedDependency
  {
      public async Task<string> UpdatePart(TestInput input)
      {
          return "this is a fallback method for update part";
      }
  }
```

## 链路跟踪

silky框架使用[SkyAPM](https://github.com/SkyAPM/SkyAPM-dotnet)实现了链路跟踪,开发者通过引入相应的配置和服务,即可实现对http请求、RPC调用、TCC分布式事务执行过程以及EFCore数据访问的调用链路跟踪。

开发者可以通过查看[链路跟踪](link-tracking)节点熟悉如何进行配置和引入相应的服务以及如何部署 **skywalking**，并通过 **skywalking** 查看调用的链路。

## 安全

在silky框架中,非常重视对安全模块的设计。

1. 通过`rpc:token`的配置,保证外部无法通过RPC端口直接访问应用服务。服务内部之间的调用均需要对`rpc:token`进行校验,如果`rpc:token`不一致，则不允许进行调用。
2. 在网关处统一实现身份认证与授权。开发者可以通过查看[身份认证与授权](identity)节点查看相关文档。
3. 开发者可以通过`GovernanceAttribute`特性来禁止外部访问某个应用服务方法。被外部禁止访问的应用服务方法只允许服务内部之间通过RPC方式进行通信。

```csharp
[Governance(ProhibitExtranet = true)]
Task<string> Delete(string name);
```

## 缓存拦截

在RPC通信过程中,通过引入缓存拦截,极大的提高了系统性能。

开发者可以通过[缓存](caching.html#缓存拦截)文档，熟悉缓存拦截的使用方法。