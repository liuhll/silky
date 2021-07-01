---
title: 示例
lang: zh-cn
---

## 简介

**Lms.Sample示例项目** 位于项目根目录下的**samples**目录下。主要为lms学习和使用者演示如何通过lms框架快速的构建一个分布式微服务应用。

**Lms.Sample示例项目** 主要由两部分构成:(1)是业务微服务应用模块;(2)网关应用。

业务微服务有三个独立的微服务应用组成:

1. 账户(Account)微服务应用模块

2. 订单(Order)微服务应用模块

3. 库存(Stock)微服务应用模块

三个独立的微服务应用主要演示了lms框架的如下特性:

1. 一个微服务应用的项目分层样例

2. 如何定义一个服务应用接口和实现该服务应用接口

3. 如何通过rpc在微服务集群内部进行通信

4. 缓存拦截的使用方式和分布式缓存接口的使用

5. 通过下订单的方式演示了分布式事务的使用

网关项目主要通过依赖各个微服务模块应用的**Application.Contract**包,从而实现对集群外部公布webapi接口。

开发者可以通过学习Lms.Sample示例项目更好的学习和入门lms框架。

开发者可以通过博文[通过lms.samples熟悉lms微服务框架的使用](/blog/lms-sample)和[lms框架分布式事务使用简介](/blog/lms-sample-order)学习和熟悉Lms.Sample示例项目。

## 如何运行示例项目

1. 进入到`lms\samples\docker-compose\infrastr`目录,通过如下命令，将Lms.Sample示例项目需要依赖的基础服务**Zookeeper**和**redis**以及**Mysql**运行起来。

```powershell
docker-compose -f .\docker-compose.mysql.yml -f .\docker-compose.redis.yml -f .\docker-compose.zookeeper.yml up -d
```

2. 实现数据库迁移

需要分别进入到各个微服务模块下的EntityFrameworkCore项目(例如:),执行如下命令:

```powershell
dotnet ef database update
```

3. 使用visual studio或是rider进行开发调式

