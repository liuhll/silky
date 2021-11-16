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

在RPC通信过程中,如果发生IO异常(`IOException`)、通信异常(`CommunicationException`)、或是服务提供者不存在调用服务的本地服务条目(服务提供者抛出`NotFindLocalServiceEntryException`异常)、超出服务提供者配置的最大处理并发量,则会根据配置的重试次数选择其他服务实例,进行调用。

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
                        .WaitAndRetryAsync(serviceEntryDescriptor.GovernanceOptions.RetryIntervalMillSeconds,
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

RPC通信过程中,在开启熔断保护的情况下,如果在连续发生n次非业务类异常(包括友好类异常、认证类异常、参数校验类异常等)，则会触发熔断保护,在一段时间内,该服务条目将不可用。

开发者通过`Governance:EnableCircuitBreaker`配置项来确定是否要开启熔断保护，通过`Governance:ExceptionsAllowedBeforeBreaking`配置项来确定在熔断保护触发前允许的异常次数(这里的异常为非业务类异常)，通过`Governance:BreakerSeconds`配置项来确定熔断的时长(单位为:秒)。同样地,熔断保护也可以通过`GovernanceAttribute`来进行配置。


```csharp
[HttpGet("{name}")]
[Governance(EnableCircuitBreaker = true,ExceptionsAllowedBeforeBreaking = 2, BreakerSeconds = 120)]
Task<TestOut> Get([HashKey]string name);
```

## 限流

## 失败回退

## 链路跟踪

## 服务保护

## 缓存拦截