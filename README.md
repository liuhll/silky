<p align="center">
  <img height="200" src="./docs/.vuepress/public/assets/logo/logo.word.svg">
</p>

# lms microservice framework
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitee.com/dotnetchina/lms/raw/main/LICENSE)
[![Commit](https://img.shields.io/github/last-commit/liuhll/lms)](https://img.shields.io/github/last-commit/liuhll/lms)
[![NuGet](https://img.shields.io/nuget/v/Silky.Lms.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Lms.Core)
[![MyGet (nightly builds)](https://img.shields.io/myget/lms-framework/vpre/Silky.Lms.Core.svg?style=flat-square)](https://www.myget.org/feed/Packages/lms-framework)
[![NuGet Download](https://img.shields.io/nuget/dt/Silky.Lms.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Lms.Core)


<div align="center">

**English | [简体中文](./README.zh-CN.md)**

</div>

## Project Introduction

The lms framework is designed to help developers quickly build a microservice development framework through simple code and configuration under the .net platform.

Build general business microservice applications through the [Generic Host](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0) of the .net framework, internal Communicate through the rpc implemented by [dotnetty/SpanNetty](https://github.com/cuteant/SpanNetty). During the message transmission process, the message is communicated within the same cluster through `rpcToken`, and the rpc communication supports ssl encryption.

Through the [web host] of .net (https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0) build a service host that provides access to the outside world ( Gateway), when the `http` request or the `ws` session request arrives at the host, the routing entry of the service cluster is resolved through the built-in middleware, and the `rpcToken` is specified, and the built-in load balancing algorithm and routing addressing and The hosts inside the cluster communicate with `rpc`.

In the communication process, LMS uses cache-based interception to realize TCC distributed transaction.


In the development and design process, the design and ideas of various excellent open source products have been borrowed and absorbed. Here, the author expresses his tribute and gratitude to the ancestors.

## getting Started

- Learn the lms framework through [Developer Documentation](http://docs.lms-fk.com/lms/).
- Use [lms.samples project](http://docs.lms-fk.com/lms/dev-docs/quick-start.html) to familiarize yourself with how to build a microservice application using the lms framework.
-Familiarize yourself with the configuration properties of the lms framework through the [Configuration](http://docs.lms-fk.com/config/) document.

## Framework Features

### Service Engine
- Responsible for the initialization process of the lms host
- Service registration and analysis
- Responsible for module analysis and registration

### Routing and parameters
- Routing analysis and maintenance of the distributed application cluster routing table through the registry
- Generate restful style WebAPI through the gateway to provide http services to the outside
- Realize the verification of input parameters through characteristics

### RPC communication
- Use [dotnetty/SpanNetty](https://github.com/cuteant/SpanNetty) as the underlying communication component
- Use [Zookeeper](https://zookeeper.apache.org) as the service registry
- Use [Castle.Core.AsyncInterceptor](https://www.nuget.org/packages/Castle.Core.AsyncInterceptor/) to generate dynamic proxy
- Support cache interception
- Support JSON, MessagePack, ProtoBuf encoding and decoding methods

### Service governance
- Support load balancing routing methods such as polling, random routing, hash consistency, etc.
- Support failure callback
-Use Policy to realize service fusing and retry
- Support service failover
- Support removing unhealthy services
- Disable external access to services through configuration support

> The service governance module will continue to be updated

### Modular Management
- Module dependency settings
- Register service through the module
- Pass module pre-initialization method or release resources

### Support distributed transactions
- Realize distributed transactions through TCC

### Support websocket communication
- Realize websocket communication through [websocket-sharp](https://github.com/sta/websocket-sharp)

### Distributed lock
- Use [RedLock.net](https://github.com/samcook/RedLock.net) to implement distributed locks