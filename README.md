<p align="center">
  <img height="200" src="./docs/.vuepress/public/assets/logo/logo.svg">
</p>

# Silky 微服务框架
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![Commit](https://img.shields.io/github/last-commit/liuhll/silky)](https://img.shields.io/github/last-commit/liuhll/silky)
[![NuGet](https://img.shields.io/nuget/v/silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![MyGet (nightly builds)](https://img.shields.io/myget/silky-framework/vpre/Silky.Core.svg?style=flat-square)](https://www.myget.org/feed/Packages/silky-framework)
[![NuGet Download](https://img.shields.io/nuget/dt/Silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2Fliuhll%2Fsilky&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)

<div align="center">

**简体中文 | [English](./README.en-US.md)**

</div>

## 给一颗星！ ⭐️

如果你喜欢这个仓库或者它对你有帮助，请给这个仓库一个星星⭐️。 这不仅有助于加强我们的社区，还有助于提高开发人员学习 Silky 框架的技能👍。 非常感谢你。

## 项目介绍

Silky框架旨在帮助开发者在.net平台下,通过简单代码和配置快速构建一个微服务开发框架。

通过.net框架的[通用主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0)构建普通业务微服务应用,内部通过[dotnetty/SpanNetty](https://github.com/cuteant/SpanNetty)实现的rpc进行通信,在消息传递过程中,通过`rpcToken`保证消息在同一个集群内部进行通信，而且rpc通信支持ssl加密。

通过.net的[web主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0)构建对外提供访问入口的服务主机(网关)，在`http`请求或是`ws`会话请求到达该主机时,通过内置的中间件解析到服务集群的路由条目,并指定`rpcToken`,通过内置的负载均衡算法和路由寻址与集群内部的主机进行`rpc`通信。

Silky在通信过程中,使用基于缓存拦截实现了TCC分布式事务。


在开发与设计过程中借鉴和吸收了各个优秀的开源产品的设计与思想。在此，作者表示对各个先辈的致敬与感谢。

## 入门

- 通过[开发者文档](http://docs.lms-fk.com/silky/)学习Silky框架。
- 通过[silky.samples项目](http://docs.lms-fk.com/silky/dev-docs/quick-start.html)熟悉如何使用Silky框架构建一个微服务应用。
- 通过[配置](http://docs.lms-fk.com/config/)文档熟悉Silky框架的相关配置属性。

## 框架特性

### 服务引擎
- 负责Silky主机的初始化过程
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

> 服务治理模块后续持续更新

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

## 产品路线图

### 目前
- [ ] 重构分布式事务,通过定时任务作业和undolog检查事务参与者的状态和完成分支事务

- [x] 新增通过[Mapster](https://github.com/MapsterMapper/Mapster)实现对象映射的模块

### 近期
- 新增身份认证与授权中间件
- 完善文档

### 未来

- 新增服务管理端(Dashboard)
- 新增模板项目和CLI命令行行工具
- 完善示例
- 使用silky框架实现一个权限管理系统

## 贡献
- 贡献的最简单的方法之一就是是参与讨论和讨论问题（issue）。你也可以通过提交的 Pull Request 代码变更作出贡献。
