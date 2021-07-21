---
title:  服务注册中心
lang: zh-cn
---

## 服务注册中心原理

在分布式系统里的注册中心。原理是将部署服务的机器地址记录到注册中心，服务消费者在有需求的时候，只需要查询注册中心，输入提供的服务名，就可以得到地址，从而发起调用。

在微服务架构下，主要有三种角色：**服务提供者（RPC Server）**、**服务消费者（RPC Client）** 和 **服务注册中心（Registry）**，三者的交互关系请看下面这张图:

![service-registry1.png](/assets/imgs/service-registry1.png)

**RPC Server** 提供服务，在启动时，根据配置文件指定的服务注册中心配置信息，向 **Registry** 注册自身的服务路由，并向**Registry**定期发送心跳汇报存活状态。

**RPC Client** 调用服务，在启动时，根据配置文件指定的服务注册中心配置信息，向 **Registry** 订阅服务，把**Registry**返回的服务节点列表缓存在本地内存中，并与 **RPC Sever** 建立连接。

当 **RPC Server** 节点发生变更时，**Registry**会同步变更，**RPC Client** 感知后会刷新本地内存中缓存的服务节点列表。

**RPC Client** 从本地缓存的服务节点列表中，基于负载均衡算法选择一台 **RPC Sever** 发起调用。

::: tip 提示
1. 对于一个微服务应用来说,在集群中,它既可以作为**RPC Sever**,也可能作为**RPC Client**，主要是看在rpc通信过程中，是提供服务的一方，还是调用服务的一方。

:::

## 服务路由

在silky框架中,服务提供者将会以**服务路由**为单位向服务注册中心注册路由信息。每一个**被实现了**的应用服务接口方法(服务条目)都会生成一条**服务路由**。

服务路由的格式如下：

```json
{
    "serviceDescriptor":{
        "id":"IAnotherApplication_IAnotherAppService_DeleteTwo_name",
        "serviceProtocol":0,
        "metadatas":{

        }
    },
    "addressDescriptors":[
        {
            "address":"172.19.16.1",
            "port":2202,
            "serviceProtocol":0
        }
    ],
    "timeStamp":1623254897
}
```

服务路由主要由三部分组成，分别为：**服务描述符(serviceDescriptor)**、**地址描述符(addressDescriptors)**、**时间戳(timeStamp)**。

### 服务描述符(serviceDescriptor)

每一个服务条目对应生成一条**服务路由**,服务路由通过服务描述符标识其唯一性。服务描述符由如下三部分组成:

| 字段 | 说明 | 备注 |
|:-----|:-----|:-----|
| id | 服务路由(描述符)Id | 具有唯一性; 生成规则:通过服务条目对应的方法的完全限定名 + 参数名 |
| serviceProtocol | 服务通信协议 | rpc通信框架中,采用的通信协议  |
| metadatas | 其他元数据 | 可以为服务路由写入(k,v)格式的元数据  |

::: warning 注意

1. 在一个微服务集群中,服务路由具有唯一性。也就是说,不允许在同一个微服务集群中, 不同微服务应用中不允许出现两个一模一样的方法(应用服务接口的完全限定名和方法名、参数名一致);

::: 

### 地址描述符(addressDescriptors)

**地址描述符**是一个数组,用于存放该服务路由(服务条目)存在的微服务应用实例的IP地址信息。

| 字段 | 说明 | 备注 |
|:-----|:-----|:-----|
| address | ip地址 | 微服务应用实例的IP地址 |
| port | rpc通信端口号 | 微服务应用通信中指定的rpc端口号  |
| serviceProtocol | 服务通信协议 | rpc通信框架中,采用的通信协议  |

::: warning 注意

1. 只有被实现的应用服务接口才会生成服务路由。

2. 在微服务应用向服务注册中心注册路由时，首先会从服务注册中心获取最新的路由信息,在内存中更新该服务路由的服务路由地址(**将该微服务应用的地址更新到地址描述符中**),并将更新后的服务路由地址注册到服务注册中心,其他微服务应用根据从服务注册中心订阅到更新后的服务路由信息后,会将服务路由信息更新到微服务的内存中缓存起来。

3. 为防止同一个微服务应用同时伸缩服务实例,在微服务应用获取或是注册服务路由的过程中会加分布式锁。

::: 

### 时间戳

`timeStamp`是指向服务注册中心更新服务路由的最后时间。


## 使用zookeeper作为服务注册中心

当前,silky支持使用zookeeper作为服务注册中心。

silky支持为微服务集群配置多个服务注册中心，您只需要在配置服务注册中心的链接字符串`registrycenter.connectionStrings`中,使用分号`;`就可以指定微服务框架的多个服务注册中心。

为方便以后扩展其他服务作为服务注册中心，您需要在为微服务应用指定配置时,显式的指定微服务注册中心类型为(`registrycenter.registryCenterType`):Zookeeper

为微服务配置服务注册中心如下所示:

```yml

registrycenter: // 服务注册中心配置节点
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 // 服务配置中心链接
  registryCenterType: Zookeeper // 注册中心类型
  connectionTimeout: 1000 // 链接超时时间(单位:ms)
  sessionTimeout: 2000 // 会话超时时间(单位:ms)
  operatingTimeout: 4000 // 操作超时时间(单位:ms)
  routePath: /services/serviceroutes

```

除此之外,使用zookeeper作为服务注册中心,还必须要依赖`ZookeeperModule`模块。

::: warning 注意

1. 默认启动服务模块(`NormHostModule`、`WebHostModule`、`WsHostModule`)均已经依赖`ZookeeperModule`模块。

::: 