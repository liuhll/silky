---
title: 配置
lang: zh-cn
---

## 介绍

silky框架支持通过`json`或是`yml`格式的配置文件,开发者可以根据需要对下面配置节点进行调整。例如:通过`appsettings.yml`对微服务应用进行统一配置,通过`appsettings.${Environment}.yml`对不同环境变量下的配置项进行设置。

## RPC通信(Rpc)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| Host | 主机Ip | 0.0.0.0 | 设置微服务应用的ip地址,"0.0.0.0"自动获取当前主机Ip地址 |
| Port |  RPC端口号 | 2200 | RPC通信使用的端口号 |
| UseLibuv | 是否启用Libuv | true |  |
| IsSsl | 是否开启Ssl | false |  |
| SslCertificateName | Ssl证书(文件)名称 |  |  |
| SslCertificatePassword | Ssl证书密码 |  |  |
| SoBacklog | dotNetty通信的SoBacklog参数 | 8192  |  |
| Token | RPC通信的密钥 |  | 不允许为空,且同一个微服务应用集群,配置的token必须一致 |
| ConnectTimeout | RPC通信过程中建立链接的超时时间 | 500 | 单位:ms |

## 使用Zookeeper作为服务中心的配置(RegistryCenter)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| Type | 服务注册中心类型 | Zookeeper  | 配置值为`Zookeeper`,标识使用 Zookeeper作为服务注册中心 |
| ConnectionTimeout | 连接超时 | 5000 | 单位(ms)  |
| SessionTimeout | session会话超时 | 8000 | 单位(ms)  |
| OperatingTimeout | 操作超时 | 10000 | 单位(ms)  |
| ConnectionStrings | 服务注册中心地址 |  | 支持多服务注册中心,通一个集群的地址使用逗号(,)分割，多个服务注册中心使用(;)进行分割。例如:`zookeeper1:2181,zookeeper1:2182,zookeeper1:2183;zookeeper2:2181,zookeeper2:2182,zookeeper2:2183` |
| RoutePath | 服务元数据的路由地址 | /services/serviceroutes | 当服务启动时会更新服务元数据和终结点地址信息 |

## 使用Nacos作为服务中心的配置(RegistryCenter)

## 使用Consul作为服务中心的配置(RegistryCenter)

## 服务治理(Governance)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| ShuntStrategy | 负载均衡策略 | Polling(轮询) | 负载均衡算法支持：Polling(轮询)、Random(随机)、HashAlgorithm(哈希一致性) |
| TimeoutMillSeconds | 执行超时时间 | 3000(ms) | 单位为(ms),超时时发生熔断，-1表示在rpc通信过程中不会超时 |
| EnableCachingInterceptor | 是否支持缓存拦截 | true | 如果配置为`false`，则缓存拦截不会生效 |
| EnableCircuitBreaker | 是否开启熔断保护 | true |  |
| ExceptionsAllowedBeforeBreaking | 开启熔断保护前允许的异常次数 | 3 | 只能是非业务类异常 |
| BreakerSeconds | 熔断保护时长 | 3 | 60 |
| AddressFuseSleepDurationSeconds | 通信异常发生时,服务实例进入休眠的时长 | 60 | 达到允许的休眠次数后,服务实例将会被下线 |
| UnHealthAddressTimesAllowedBeforeRemoving | 允许不健康服务实例进入的休眠次数 | 0 |  |
| RetryIntervalMillSeconds | 服务重试的间隔时长 | 50 |  |
| MaxConcurrentHandlingCount | 允许的最大并发处理量  | 50 | 服务实例达到最大并发处理量,将抛出异常 |
| EnableHeartbeat | 是否允许心跳检测  | false | 开启心跳检测后,服务之间在建立链接后,将会通过发送心跳包保证dotnetty的链接存活 |
| HeartbeatWatchIntervalSeconds | 心跳包的时间间隔  | 300 | 最小值为`60`s，如果小于60,则会被设置为60 |


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


## 网关(Gateway)

| 属性 | 说明 | 缺省值 |  备注 |
|:-----|:-----|:-----|:------|
| ResponseContentType | http请求的响应内容类型 |  |  |
| JwtSecret | 签发token的jwt密钥 |  | 必须与签发token应用配置的保持一致  |
| IgnoreWrapperPathPatterns |  不需要包装的返回值的接口或是请求路径 | 忽略静态资源相关  | 数据类型为数组  |
               


::: warning

该配置只有配置在网关应用才会生效。

:::