---
title: silky框架介绍
lang: zh-cn
---

## 背景介绍

silky框架旨在帮助开发者在.net平台下,通过简单代码和配置快速构建一个微服务开发框架。

通过.net框架的[通用主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0)构建普通业务微服务应用,内部通过[dotnetty/SpanNetty](https://github.com/cuteant/SpanNetty)实现的rpc进行通信,在消息传递过程中,通过`rpcToken`保证消息在同一个集群内部进行通信，而且rpc通信支持ssl加密。

通过.net的[web主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0)构建对外提供访问入口的服务主机(网关)，在`http`请求或是`ws`会话请求到达该主机时,通过内置的中间件解析到服务集群的路由条目,并指定`rpcToken`,通过内置的负载均衡算法和路由寻址与集群内部的主机进行`rpc`通信。

silky在通信过程中,使用基于缓存拦截实现了TCC分布式事务。


在开发与设计过程中借鉴和吸收了各个优秀的开源产品的设计与思想。在此，作者表示对各个先辈的致敬与感谢。

为方便开发者学习与表达对前辈的谢意,如下对silky各个模块的设计思想来源做出说明:

**服务引擎与IOC容器**: 该模块主要借鉴了[nopCommerce](https://github.com/nopSolutions/nopCommerce/)

**模块管理**: 该模块的设计主要借鉴[ABP](https://github.com/abpframework/abp)

**RPC通信**: 该模块的设计主要借鉴了[Surging](https://github.com/fanliang11/surging)和[RabbitCloud](https://github.com/RabbitTeam/RabbitCloud)

**路由与参数解析**: 该模块借鉴了[aspnetcore](https://github.com/dotnet/aspnetcore)

**缓存拦截**: 该模块的设计主要借鉴了[Surging](https://github.com/fanliang11/surging)的设计思想

**动态代理**: 该模块的设计主要借鉴了[ABP](https://github.com/abpframework/abp)

**分布式缓存**: 该模块的设计主要借鉴了[ABP](https://github.com/abpframework/abp)

**分布式事务**: 该模块的设计思想主要借鉴了[hmily](https://github.com/dromara/hmily)

**zookeeper客户端SDK**: 该模块使用了[zookeeper-client](https://github.com/RabbitTeam/zookeeper-client)

**WebSocket通信**: 该模块借鉴了[Surging](https://github.com/fanliang11/surging)和[Ocelot](https://github.com/ThreeMammals/Ocelot)

**分布式锁**: 分布式锁使用了[RedLock.net](https://github.com/samcook/RedLock.net)

**实体映射**: 该模块使用了[AutoMapper](https://github.com/AutoMapper/AutoMapper)

## 框架特性

### 服务引擎
- 负责silky主机的初始化过程
- 服务注册与解析
- 负责模块解析与注册

### 路由与参数
- 路由的解析与通过注册中心的维护分布式应用集群路由表
- 通过网关生成restful风格的WebAPI对外部提供http服务
- 通过特性实现输入参数的校验

### RPC通信
- 使用[dotnetty/SpanNetty](https://github.com/cuteant/SpanNetty)作为底层通信组件
- 使用[Zookeeper](https://zookeeper.apache.org)作为服务注册中心
- 使用[Castle.Core.AsyncInterceptor](https://www.nuget.org/packages/Castle.Core.AsyncInterceptor/)生成动态代理
- 支持缓存拦截
- 支持JSON、MessagePack、ProtoBuf编解码方式

### 服务治理
- 支持轮询、随机路由、哈希一致性等负载均衡路由方式
- 支持失败回调
- 使用Policy实现服务熔断与重试
- 支持服务故障转移
- 支持移除不健康的服务
- 通过配置支持禁止服务被外部访问

:::tip
服务治理模块后续持续更新
:::

### 模块化管理
- 模块的依赖设置
- 通过模块注册服务
- 通过模块预初始化方法或释放资源

### 支持分布式事务
- 通过TCC方式实现分布式事务

### 支持websocket通信
- 通过[websocket-sharp](https://github.com/sta/websocket-sharp)实现websocket通信

### 分布式锁
- 使用[RedLock.net](https://github.com/samcook/RedLock.net)实现分布式锁

## 开源地址
- github: [https://github.com/liuhll/silky](https://github.com/liuhll/silky)
- gitee: [https://gitee.com/liuhll2/silky](https://gitee.com/liuhll2/silky)

## 贡献
- 贡献的最简单的方法之一就是是参与讨论和讨论问题（issue）。你也可以通过提交的 Pull Request 代码变更作出贡献。
- 您也可以加入QQ群(934306776)参与silky框架的学习讨论。 

  ![qq-group.jpg](/assets/imgs/qq-group.jpg)