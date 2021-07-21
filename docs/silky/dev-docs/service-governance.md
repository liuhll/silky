---
title: 服务治理
lang: zh-cn
---

## 概念

服务治理是主要针对分布式服务框架，微服务，处理服务调用之间的关系，服务发布和发现（谁是提供者，谁是消费者，要注册到哪里），出了故障谁调用谁，服务的参数都有哪些约束,如何保证服务的质量？如何服务降级和熔断？怎么让服务受到监控，提高机器的利用率?

微服务有哪些问题需要治理？

1. **服务注册与发现**: 单体服务拆分为微服务后，如果微服务之间存在调用依赖，就需要得到目标服务的服务地址，也就是微服务治理的”服务发现“。要完成服务发现，就需要将服务信息存储到某个载体，载体本身即是微服务治理的*服务注册中心*，而存储到载体的动作即是*服务注册*。

2. **可观测性**: 微服务由于较单体应用有了更多的部署载体，需要对众多服务间的调用关系、状态有清晰的掌控。可观测性就包括了调用拓扑关系、监控（Metrics）、日志（Logging）、调用追踪（Trace）等。

3. **流量管理**: 由于微服务本身存在不同版本，在版本更迭过程中，需要对微服务间调用进行控制，以完成微服务版本更迭的平滑。这一过程中需要根据流量的特征（访问参数等）、百分比向不同版本服务分发，这也孵化出灰度发布、蓝绿发布、A/B测试等服务治理的细分主题。

4. **服务容错**: 任何服务都不能保证100%不出问题，生产环境复杂多变，服务运行过程中不可避免的发生各种故障（宕机、过载等等），工程师能够做的是在故障发生时尽可能降低影响范围、尽快恢复正常服务,需要引入「熔断、隔离、限流和降级、超时机制」等「服务容错」机制来保证服务持续可用性。

4. **安全**: 不同微服务承载自身独有的业务职责，对于业务敏感的微服务，需要对其他服务的访问进行认证与鉴权，也就是安全问题。

5. **控制**： 对服务治理能力充分建设后，就需要有足够的控制能力，能实时进行服务治理策略向微服务分发。

6. **服务本身的治理**: 确保微服务主机的健康,有能力将不健康节点从微服务集群中移除。

## Silky框架的服务治理

silky框架的服务治理主要以**服务条目**为基本单位，框架为每个服务条目的治理属性指定了缺省值,开发者也可以通过**配置文件统一配置**或是通过**GovernanceAttribute**特性对服务治理属性进行设置。

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


### 统一配置

开发者可以通过配置文件的`Governance`节点对微服务的治理进行统一配置。如果在配置文件中不对服务治理进行配置,那么，在rpc通信过程中,服务治理的属性值使用缺省值。

对服务治理的配置如下述所示:

```yaml
governance:
  addressSelectorMode: Random
  executionTimeout: 3000
  maxConcurrent: 500
```

### 通过`GovernanceAttribute`特性

开发者可以通过`GovernanceAttribute`特性对应用服务接口方法进行标识，通过`GovernanceAttribute`特性的属性对该服务条目的治理方式进行调整。

例如:

```csharp
[Governance(FallBackType = typeof(UpdatePartFallBack),ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
Task<string> UpdatePart(TestInput input);

```

### 失败回调

应用服务接口方法可以通过`GovernanceAttribute`特性的`FallBackType`属性指定失败回调类型。指定的失败回调类型必须是一个非抽象的类,且必须继承自`IFallbackInvoker<ReturnType>`。

例如:上述应用程序接口方法指定的失败回调类型`UpdatePartFallBack`定义如下所示:

```csharp
public class UpdatePartFallBack : IFallbackInvoker<string> 
//泛形类型与应用程序接口的返回值类型保持一致
{
    public async Task<string> Invoke(IDictionary<string, object> parameters)
    {
        return "UpdatePartFallBack";
    }
}

```

:::warning

应用服务接口方法的治理属性的优先级为: `GovernanceAttribute`特性 > 配置 > 缺省值

:::