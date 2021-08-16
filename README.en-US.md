<p align="center">
  <img height="200" src="./docs/.vuepress/public/assets/logo/logo.svg">
</p>

# Silky Microservice Framework
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![Commit](https://img.shields.io/github/last-commit/liuhll/silky)](https://img.shields.io/github/last-commit/liuhll/silky)
[![NuGet](https://img.shields.io/nuget/v/silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![MyGet (nightly builds)](https://img.shields.io/myget/silky-preview/vpre/Silky.Core.svg?style=flat-square)](https://www.myget.org/feed/Packages/silky-preview)
[![NuGet Download](https://img.shields.io/nuget/dt/Silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2Fliuhll%2Fsilky&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)

<div align="center">

**English | [简体中文](./README.md)**

</div>

## Give a Star! ⭐️

If you liked this repo or if it helped you, please give a star ⭐️ for this repository. That will not only help strengthen our community but also improve the skills of developers to learn Silky framework 👍. Thank you very much.

## Project Introduction

The Silky framework is designed to help developers quickly build a microservice development framework through simple code and configuration under the .net platform.

Build general business microservice applications through the [Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0) of the .net framework, internal Communicate through the rpc implemented by [dotnetty](https://github.com/Azure/DotNetty). During the message transmission process, the message is communicated within the same cluster through `rpcToken`, and the rpc communication supports ssl encryption.

Through the [Web Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-5.0) of .net  build a service host that provides access to the outside world ( Gateway), when the `http` request or the `ws` session request arrives at the host, the routing entry of the service cluster is resolved through the built-in middleware, and the `rpcToken` is specified, and the built-in load balancing algorithm and routing addressing and The hosts inside the cluster communicate with `rpc`.

In the process of rpc communication, Silky uses interceptors and Undo Log logs to implement TCC distributed transactions to ensure the final consistency of data.


In the development and design process, the design and ideas of various excellent open source products have been borrowed and absorbed. Here, the author expresses his tribute and gratitude to the ancestors.

## Getting Started

- Learn the Silky framework through [Developer Documentation](http://docs.silky-fk.com/silky/).
- Use [silky.samples project](http://docs.silky-fk.com/silky/dev-docs/quick-start.html) to familiarize yourself with how to build a microservice application using the Silky framework.
- Familiarize yourself with the configuration properties of the Silky framework through the [Configuration](http://docs.silky-fk.com/config/) document.

## Framework Features

### Agent Host
-General agent host: business microservice host, the microservice cluster communicates through the RPC protocol within the microservice cluster, and cannot communicate with the outside of the cluster
-Web agent host: Provide Http access port to the outside of the cluster. When the http request arrives, it communicates with the host inside the cluster through the RPC protocol, which is usually used as a gateway
-WebSocket agent host: a business microservice host with the ability to provide websocket communication

### Service Engine
- Responsible for the initialization process of the Silky host
- Service registration and analysis
- Responsible for module analysis and loading

### Modular Management
- Module dependency settings
- Register service through the module
- Pass module pre-initialization method or release resources

### Routing and WebAPI
- Routing analysis and maintenance of the distributed application cluster routing table through the registry
- Generate restful style through service entries
- Support for generating online webapi documents through Swagger
- Support performance monitoring of http requests through Miniprofiler

### Parameter Verification
- Automatically verify parameters through filters during RPC call
- Support the verification of input parameters through features
- Support input parameter verification through Fluent

### RPC Communication
- Use [dotnetty](https://github.com/Azure/DotNetty) as the underlying communication component
- Use [Zookeeper](https://zookeeper.apache.org) as the service registry
- Use [Castle.Core.AsyncInterceptor](https://www.nuget.org/packages/Castle.Core.AsyncInterceptor/) to generate dynamic proxy
- Support cache interception during service call
- Support JSON, MessagePack, ProtoBuf encoding and decoding methods
- Support custom filters during Rpc call (client filter: `IClientFilter`, server filter: `IServerFilter`)

### Service Governance
- Support load balancing routing methods such as polling, random routing, hash consistency, etc.
- Support failure callback
- Use [Polly](https://github.com/App-vNext/Polly) to realize service fusing and retry
- Support service failover
- Support removing unhealthy services
- Disable external access to services through configuration support

> The service governance module will continue to be updated

### Support Distributed Transactions
- Realize distributed transactions through TCC
- Ensure the final consistency of data through timed operations and Undolog

### Link Tracking
- Realize link tracking of calls between microservices through SkyApm

### Identity Authentication and Authorization
- Realize identity authentication based on Jwt
- Support custom interface authentication

### Data Access
- Use EfCore to implement data access components

### Support Websocket Communication
- Implement a module that supports websocket communication through [websocket-sharp](https://github.com/sta/websocket-sharp)

### Distributed lock
- Use [madelson/DistributedLock](https://github.com/madelson/DistributedLock) to implement distributed locks

## RoadMap

### Recent
- Improve documentation

### Future
- Added service management dashboard
- Added CLI command line tool
- Use silky framework to implement a rights management system
- Support file upload and download

## Contribute
- One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting Pull Request code changes.
