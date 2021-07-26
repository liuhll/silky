<p align="center">
  <img height="200" src="./docs/.vuepress/public/assets/logo/logo.svg">
</p>

# Silky Microservice Framework
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![Commit](https://img.shields.io/github/last-commit/liuhll/silky)](https://img.shields.io/github/last-commit/liuhll/silky)
[![NuGet](https://img.shields.io/nuget/v/silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![MyGet (nightly builds)](https://img.shields.io/myget/silky-framework/vpre/Silky.Core.svg?style=flat-square)](https://www.myget.org/feed/Packages/silky-framework)
[![NuGet Download](https://img.shields.io/nuget/dt/Silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2Fliuhll%2Fsilky&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)

<div align="center">

**English | [简体中文](./README.zh-CN.md)**

</div>

## Project Introduction

The Silky framework is designed to help developers quickly build a microservice development framework through simple code and configuration under the .net platform.

Build general business microservice applications through the [Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0) of the .net framework, internal Communicate through the rpc implemented by [dotnetty/SpanNetty](https://github.com/cuteant/SpanNetty). During the message transmission process, the message is communicated within the same cluster through `rpcToken`, and the rpc communication supports ssl encryption.

Through the [Web Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0) of .net  build a service host that provides access to the outside world ( Gateway), when the `http` request or the `ws` session request arrives at the host, the routing entry of the service cluster is resolved through the built-in middleware, and the `rpcToken` is specified, and the built-in load balancing algorithm and routing addressing and The hosts inside the cluster communicate with `rpc`.

In the communication process, Silky uses cache-based interception to realize TCC distributed transaction.


In the development and design process, the design and ideas of various excellent open source products have been borrowed and absorbed. Here, the author expresses his tribute and gratitude to the ancestors.

## Getting Started

- Learn the Silky framework through [Developer Documentation](http://docs.lms-fk.com/silky/).
- Use [silky.samples project](http://docs.lms-fk.com/silky/dev-docs/quick-start.html) to familiarize yourself with how to build a microservice application using the Silky framework.
- Familiarize yourself with the configuration properties of the Silky framework through the [Configuration](http://docs.lms-fk.com/config/) document.

## Framework Features

### Service Engine
- Responsible for the initialization process of the Silky host
- Service registration and resolve
- Responsible for module resolve and registration

### Routing And Parameters
- Routing resolve and maintenance of the distributed application cluster routing table through the registry
- Generate restful style WebAPI through the gateway to provide http services to the outside
- Realize the verification of input parameters through characteristics

### RPC Communication
- Use [dotnetty/SpanNetty](https://github.com/cuteant/SpanNetty) as the underlying communication component
- Use [Zookeeper](https://zookeeper.apache.org) as the service registry
- Use [Castle.Core.AsyncInterceptor](https://www.nuget.org/packages/Castle.Core.AsyncInterceptor/) to generate dynamic proxy
- Support cache interception
- Support JSON, MessagePack, ProtoBuf encoding and decoding methods

### Service Governance
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

### Support Distributed Transactions
- Realize distributed transactions through TCC

### Support Websocket Communication
- Realize websocket communication through [websocket-sharp](https://github.com/sta/websocket-sharp)

### Distributed Lock
- Use [RedLock.net](https://github.com/samcook/RedLock.net) to implement distributed locks

## RoadMap
### Currently
- [ ] Refactor distributed transactions, check the status of transaction participants and complete branch transactions through timing tasks and undolog

- [ ] Added a module for implementing object mapping through [Mapster](https://github.com/MapsterMapper/Mapster)

### Recent
- Added identity authentication and authorization middleware
- Improve documentation

### Future

- Added service management dashboard
- Added template project and CLI command line tool
- Complete example
- Use silky framework to implement a rights management system

## Contribute
- One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting Pull Request code changes.
- You can also join the QQ group (934306776) to participate in the discussion of the Silky framework.

    ![qq-group.jpg](./docs/.vuepress/assets/public/../../public/assets/imgs/qq-group.jpg)