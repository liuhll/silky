---
title: 配置
lang: zh-cn
---

## 介绍

silky框架支持通过`json`或是`yml`格式的配置文件,开发者可以根据需要对下面配置节点进行调整。例如:通过`appsettings.yml`对微服务应用进行统一配置,通过`appsettings.${Environment}.yml`对不同环境变量下的配置项进行设置。

## RPC通信(Rpc)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| Host | 主机Ip | 0.0.0.0 | 设置微服务托管记住的ip地址,"0.0.0.0"自动获取当前主机Ip地址 |
| Port | Rpc端口号 | 2200 | rpc通信使用的端口号 |
| UseLibuv | 是否启用Libuv | true |  |
| IsSsl | 是否开启Ssl | false |  |
| SslCertificateName | Ssl证书(文件)名称 |  |  |
| SslCertificatePassword | Ssl证书密码 |  |  |
| SoBacklog | dotNetty通信的SoBacklog参数 | 8192  |  |
| RemoveUnhealthServer | 是否移除不健康的服务 | true |  |
| Token | rpc通信的密钥 |  | 不允许为空,且同一个微服务应用集群,配置的token必须一致 |
| ConnectTimeout | 与服务提供者连接的超时时间 | 500 | 单位:ms |

## 服务注册中心(RegistryCenter)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| RegistryCenterType | 服务注册中心类型 | RegistryCenterType.Zookeepe | 当前仅实现了zookeeper作为服务注册中心 |
| ConnectionTimeout | 连接超时 | 1000 | 单位(ms)  |
| SessionTimeout | session会话超时 | 2000 | 单位(ms)  |
| OperatingTimeout | 操作超时 | 4000 | 单位(ms)  |
| ConnectionStrings | 服务注册中心地址 |  | 支持多服务注册中心,通一个集群的地址使用逗号(,)分割，多个服务注册中心使用(;)进行分割。例如:`zookeeper1:2181,zookeeper1:2182,zookeeper1:2183;zookeeper2:2181,zookeeper2:2182,zookeeper2:2183` |
| RoutePath | 服务条目的路由地址 | /services/serviceroutes | 当服务启动时会自动注册路由信息到服务注册中心 |
| MqttPtah | Mqtt协议的服务条目的路由地址 | /services/serviceroutes | 暂未实现 |
| HealthCheckInterval |  | | 暂未实现心跳检查 |

## 服务治理(Governance)

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


## 分布式缓存(DistributedCache)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| HideErrors | 使用缓存移除时,是否隐藏错误 | false |  |
| KeyPrefix | 缓存key的前缀 | "" | 空字符串 |

Redis子节点有如下配置:


| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| IsEnabled | 是否使用redis服务作为缓存服务 | false |  |
| Configuration | redis缓存服务链接字符串 |  | Redis.IsEnabled设置为`true`时有效 |

GlobalCacheEntryOptions子节点有如下配置:

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| AbsoluteExpiration |  绝对到期日期 |  | 获取或设置缓存项的绝对到期日期  |
| AbsoluteExpirationRelativeToNow | 相对到期时间  |  | 获取或设置相对于当前时间的绝对到期时间 |
| SlidingExpiration |  | 20min | 获取或设置缓存项在被删除之前可以处于停用状态（例如不被访问）的时长。 这不会将项生存期延长到超过绝对到期时间（如果已设置）  |

## 分布式锁(Lock)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| LockRedisConnection | 分布式锁服务链接 |  | 使用redis作为分布式锁服务 |
| DefaultExpiry | 默认锁操作超时时间 | 120 | 单位(s)  |
| Wait | 等待分布式锁时间 | 30 | 单位(s) |
| Retry | 重试次数 | 1 |  |

## 网关(Gateway)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| DisplayFullErrorStack | 是否显示完整的错误堆栈 | false |  |
| WrapResult | 是否包装返回结果 | false |  |

::: warning

该配置只有配置在网关应用才会生效。

:::