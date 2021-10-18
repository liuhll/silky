<p align="center">
  <img height="200" src="./docs/.vuepress/public/assets/logo/logo.svg">
</p>

# Silky 微服务框架
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![Commit](https://img.shields.io/github/last-commit/liuhll/silky)](https://img.shields.io/github/last-commit/liuhll/silky)
[![NuGet](https://img.shields.io/nuget/v/silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![MyGet (nightly builds)](https://img.shields.io/myget/silky-preview/vpre/Silky.Core.svg?style=flat-square)](https://www.myget.org/feed/Packages/silky-preview)
[![NuGet Download](https://img.shields.io/nuget/dt/Silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2Fliuhll%2Fsilky&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)

<div align="center">

**简体中文 | [English](./README.en-US.md)**

</div>

## 项目介绍

Silky框架旨在帮助开发者在.net平台下,通过简单代码和配置快速构建一个微服务开发框架。

Silky 通过 .net core的[主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/?view=aspnetcore-5.0&tabs=macos#host)来托管微服务应用。通过 Asp.Net Core 提供的http服务接受外界请求，转发到后端的微服务应用，服务内部通过[DotNetty](https://github.com/Azure/DotNetty)实现的`SilkyRpc`框架进行通信。

## 入门

- 通过[开发者文档](http://docs.silky-fk.com/silky/)学习Silky框架。
- 通过[silky.samples项目](http://docs.silky-fk.com/silky/dev-docs/quick-start.html)熟悉如何使用Silky框架构建一个微服务应用。
- 通过[配置](http://docs.silky-fk.com/config/)文档熟悉Silky框架的相关配置属性。

## 框架特性

- 面向接口代理的高性能RPC调用
- 服务自动注册和发现,支持Zookeeper、Consul、Nacos作为服务注册中心
- 智能容错和负载均衡，强大的服务治理能力
- 支持缓冲拦截
- 高度可扩展能力
- 支持分布式事务
- 流量监控
- 通过SkyApm进行链路跟踪
- 通过Swagger生成在线API文档


## 快速开始

### 1. 构建主机

### 2. 定义一个服务接口

### 3. 提供者实现服务

### 4. 消费者通过RPC远程调用服务

## 微服务应用分层

## 视频教程


## 贡献
- 贡献的最简单的方法之一就是是参与讨论和讨论问题（issue）。你也可以通过提交的 Pull Request 代码变更作出贡献。
