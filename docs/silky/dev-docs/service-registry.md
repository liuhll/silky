---
title:  服务注册中心
lang: zh-cn
---

## 服务注册中心简介

在分布式系统中,服务注册中心的作用是将部署服务实例的机器地址以及其它元数据记录到注册中心,服务消费者在有需求的时候，通过查询注册中心，输入提供的服务名，获取到地址，从而发起调用。

在微服务架构下，主要有三种角色：**服务提供者（RPC Server）**、**服务消费者（RPC Client）** 和 **服务注册中心（Registry）**，三者的交互关系请看下面这张图:

![service-registry1.png](/assets/imgs/service-registry1.png)

**RPC Server** 提供服务，在启动时，根据配置文件指定的服务注册中心配置信息，向 **Registry** 注册自身的服务路由，并向**Registry**定期发送心跳汇报存活状态。

**RPC Client** 调用服务，在启动时，根据配置文件指定的服务注册中心配置信息，向 **Registry** 订阅服务，把**Registry**返回的服务节点列表缓存在本地内存中，并与 **RPC Sever** 建立连接。

当 **RPC Server** 节点发生变更时，**Registry**会同步变更，**RPC Client** 感知后会刷新本地内存中缓存的服务节点列表。

**RPC Client** 从本地缓存的服务节点列表中，基于负载均衡算法选择一台 **RPC Sever** 发起调用。

::: tip 提示

1. 对于一个微服务应用来说,在集群中,它既可以作为**RPC Sever**,也可能作为**RPC Client**，主要是看在rpc通信过程中，是提供服务的一方，还是调用服务的一方。

:::

当前, silky微服务框架支持使用 **Zookeeper** 、 **Nacos** 、 **Consul** 作为服务注册中心,开发者可以选择熟悉的服务中间件作为服务注册中心。

## 服务元数据

在silky框架中,服务提供者向服务注册中心注册的数据被称为: **服务元数据** 。服务元数据主要四部分组成:

1. **hostName** : 用于描述服务提供者的名称,为构建主机的包名称
2. **services** : 该服务提供者所提供的应用服务信息,是一个数组,包括：服务Id,服务名称,服务协议,服务条目，元数据等信息
3. **timeStamp** : 更新服务元数据的时间戳
4. **endpoints** : 该服务实例的终结点,是一个数组。不同服务注册中心注册服务实例的终结点不同


如果使用 **Zookeeper** 作为服务注册中心, 所有的服务元数据将会被维护到该主机所在节点下, 注册到 **Zookeeper** 服务注册中心的服务元数据如下:

```json
{
    "hostName":"DemoHost",
    "services":[
        {
            "id":"Demo.Application.Contracts.System.ISystemAppService",
            "serviceName":"SystemAppService",
            "serviceProtocol":0,
            "serviceEntries":[
                {
                    "id":"Demo.Application.Contracts.System.ISystemAppService.GetInfo_Get",
                    "serviceId":"Demo.Application.Contracts.System.ISystemAppService",
                    "serviceName":"SystemAppService",
                    "method":"GetInfo",
                    "webApi":"api/system/demo/info",
                    "httpMethod":0,
                    "serviceProtocol":0,
                    "metadatas":{

                    },
                    "prohibitExtranet":false,
                    "isAllowAnonymous":true,
                    "isDistributeTransaction":false
                }
            ],
            "metadatas":{

            }
        }
    ],
    "endpoints":[
        {
            "host":"172.26.144.1",
            "port":2200,
            "processorTime":2578.125,
            "timeStamp":1636464575,
            "serviceProtocol":0
        }
    ],
    "timeStamp":1636464576
}
```

如果服务注册中心是 **Consul** 或是 **Nacos**, 服务实例的终结点则会单独注册和维护, 服务元数据格式如下:

```json
{
    "hostName":"DemoHost",
    "services":[
        {
            "id":"Demo.Application.Contracts.System.ISystemAppService",
            "serviceName":"SystemAppService",
            "serviceProtocol":0,
            "serviceEntries":[
                {
                    "id":"Demo.Application.Contracts.System.ISystemAppService.GetInfo_Get",
                    "serviceId":"Demo.Application.Contracts.System.ISystemAppService",
                    "serviceName":"SystemAppService",
                    "method":"GetInfo",
                    "webApi":"api/system/demo/info",
                    "httpMethod":0,
                    "serviceProtocol":0,
                    "metadatas":{

                    },
                    "prohibitExtranet":false,
                    "isAllowAnonymous":true,
                    "isDistributeTransaction":false
                }
            ],
            "metadatas":{

            }
        }
    ],
    "timeStamp":1636464576
}
```

### 主机名称(hostName)

**hostName** 用于描述服务提供者的名称,在向服务注册中心注册服务的过程中,应用会判断服务注册中心是否存在该应用的服务元数据,如果不存在,则创建相应的节点,并添加相应的服务元数据;如果已经存在相应的服务节点,则会更新服务元数据,其他服务提供者的实例从服务注册中心获取到服务元数据,并更新本地内存的服务元数据。

### 服务列表(services)

该属性包含该应用所支持的服务列表,如果服务注册中心的服务列表被更新,其他服务实例也会从服务注册中心获取,并更新到本地内存。

服务列表包括：服务Id,服务名称,服务协议,服务条目，元数据等信息

| 字段 | 说明 | 备注 |
|:-----|:-----|:-----|
| id | 服务Id | 具有唯一性;服务接口定义的完全限定名 |
| serviceName | 服务名称 |  |
| serviceProtocol | 服务通信协议 | rpc通信框架中,采用的通信协议  |
| serviceEntries | 该服务支持的服务条目(即：应用服务定义的方法) | 数据类型为数组  |
| serviceEntries.id | 服务条目Id | 方法的完全限定名 + 参数名 + Http方法名  |
| serviceEntries.serviceId | 服务Id |  |
| serviceEntries.serviceName | 服务名称 |  |
| serviceEntries.method | 服务条目对应的方法名称 |  |
| serviceEntries.webApi | 生成的webapi 地址 | 如果被禁止访问外网则为空  |
| serviceEntries.httpMethod | 生成的webapi的请求地址 | 如果被禁止访问外网则为空  |
| serviceEntries.serviceProtocol |  rpc通信框架中,采用的通信协议 |   |
| serviceEntries.metadatas |  服务条目的元数据 | 可以为服务条目写入(k,v)格式的元数据   |
| metadatas | 服务的其他元数据 | 可以为服务写入(k,v)格式的元数据  |

::: warning 注意

1. 在一个微服务集群中,服务条目具有唯一性。也就是说,不允许在同一个微服务集群中, 不同微服务应用中不允许出现两个一模一样的方法(应用服务接口的完全限定名和方法名、参数名一致);
2. 只有被实现的应用服务才会被注册到服务注册中心。
::: 

### 终结点

**endpoints** 是用来描述该微服务的服务实例的地址信息。

一个服务实例可能存在多个终结点,如：使用webhost构建的微服务应用(存在web地址终结点和rpc终结点地址)；构建支持websocket服务的微服务应用(存在websocket服务地址终结点和rpc终结点地址)。

使用不同的服务注册中心,注册终结点可能会做不同的处理。

1. 如果使用 **Zookeeper** 作为服务注册中心, 服务实例的终结点将被更新到该服务提供者对应节点数据的 `endpoints` 属性,也就是说,`endpoints`将作为服务元数据的一个属性。

```json
{
    "hostName": "DemoHost",
    "services": ["..."],
    "timeStamp": 1636464576,
    "endpoints":[
        {
            "host":"localhost",
            "port":5000,
            "processorTime":2984.375,
            "timeStamp":1636464576,
            "serviceProtocol":4
        },
        {
            "host":"172.26.144.1",
            "port":2200,
            "processorTime":2578.125,
            "timeStamp":1636464575,
            "serviceProtocol":0
        }
    ]
}
```

2. 如果使用 **Consul** 作为服务注册中心, 服务实例将会被注册到 **Services** 节点,并且只会注册协议为`TCP`的终结点,服务实例的其他协议的将会以元数据的方式添加到元数据

![service-registry2.png](/assets/imgs/service-registry2.png)

![service-registry3.png](/assets/imgs/service-registry3.png)

3. 如果使用 **Nacos** 作为服务注册中心, 服务将会被注册到服务列表节点,与使用 **Consul** 作为服务注册中心相同,只会注册协议为`TCP`的终结点,服务实例的其他协议的将会以元数据的方式添加到元数据

![service-registry4.png](/assets/imgs/service-registry4.png)

![service-registry5.png](/assets/imgs/service-registry5.png)

终结点的属性如下所述:

| 字段 | 说明 | 备注 |
|:-----|:-----|:-----|
| host | 对应的主机地址 | 微服务应用的Ip内网地址 |
| port | 端口号 |  |
| processorTime | CPU使使用率  |  |
| timeStamp | 注册时间戳  |  |
| serviceProtocol | 服务协议  |  |

### 时间戳

`timeStamp`是指向服务注册中心更新服务元数据的时间戳。

## 使用Zookeeper作为服务注册中心

silky支持使用 **Zookeeper** 作为服务注册中心。

silky支持为微服务集群配置多个 **Zookeeper** 服务注册中心，您只需要在配置服务注册中心的链接字符串`registrycenter.connectionStrings`中,使用分号`;`就可以指定微服务框架的多个服务注册中心。

使用 **Zookeeper** 作为服务注册中心需要在配置文件中,在`registrycenter`配置节点下,将服务注册中心的类型`type`设置为: `Zookeeper`,通过`connectionStrings`属性配置服务中心的链接。同时可以通过其他属性配置服务注册中心的链接属性。

```yml

registrycenter: // 服务注册中心配置节点
  type: Zookeeper
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 // 服务配置中心链接
  connectionTimeout: 1000 // 链接超时时间(单位:ms)
  sessionTimeout: 2000 // 会话超时时间(单位:ms)
  operatingTimeout: 4000 // 操作超时时间(单位:ms)
  routePath: /services/serviceroutes

```
一般地,使用 **Zookeeper** 作为服务注册中心我们只需要指定服务注册中心类型和链接字符串即可,其他属性(如链接超时时间、会话时间、操作超时时间、注册的路由地址等)均提供了缺省值。

```yml

registrycenter: // 服务注册中心配置节点
  type: Zookeeper
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 // 服务配置中心链接

```

silky框架提供了使用 docker-compose 来编排 Zookeeper 的 [编排文件](https://raw.githubusercontent.com/liuhll/silky/main/samples/docker-compose/infrastr/docker-compose.zookeeper.yml), 开发者在获取 silky源代码后,进入到 `./samples/docker-compose/infrastr` 目录后,执行如下命令,就可以构建一个 **Zookeeper** 服务集群。

```powershell
docker-compose -f docker-compose.zookeeper.yml up -d
```

## 使用Nacos作为服务注册中心

silky支持使用 **Nacos** 作为服务注册中心。

如果使用 **Nacos** 作为服务注册中心，需要将 `registrycenter:type` 配置设置为:`Nacos`,再加上 **Nacos** 的其他配置属性。开发者可以参考[Nacos文档](https://nacos.io/zh-cn/docs/what-is-nacos.html)以及[nacos-sdk-csharp](https://.readthedocs.io/en/latest/introduction/gettingstarted.html) 来熟悉 **Nacos** 的配置和使用。

使用 **Nacos** 作为服务注册中心的配置如下:

```yml
registrycenter:
  type: Nacos
  namespace: silky
  serverAddresses:
    - http://127.0.0.1:8848/
    - http://127.0.0.1:8849/
    - http://127.0.0.1:8850/
  userName: nacos
  password: nacos
  groupName: DEFAULT_GROUP
  clusterName: DEFAULT
  registerEnabled: true
  instanceEnabled: true
  namingUseRpc: true
```

关于 Nacas服务的搭建可以参考[Nacos Docker 快速开始](https://nacos.io/zh-cn/docs/quick-start-docker.html)。当然, silky框架也提供了使用 **docker-compose** 来编排 **Nacos** 服务的文件，开发者在获取源代码后,进入到 `./samples/docker-compose/infrastr` 目录后,执行如下命令,就可以构建一个 **Nacos** 服务集群。

```powershell
docker-compose -f docker-compose.nacos.cluster-hostname.yaml up -d
```

## 使用Consul作为服务注册中心

silky支持使用 **Consul** 作为服务注册中心。

如果使用 **Consul** 作为服务注册中心，需要将 `registrycenter:type` 配置设置为:`Consul`,再加上 **Consul** 的其他配置属性。开发者可以参考[Consul文档](https://www.consul.io/docs)以及[consuldotnet](https://github.com/G-Research/consuldotnet) 来熟悉 **Consul** 的配置和使用。

使用 **Consul** 作为服务注册中心的配置如下:

```yml
registrycenter:
  type: Consul
  address: http://127.0.0.1:8500
  datacenter: dc1 # 缺省值为 dc1
  token：""  # 如果consul服务设置了token，则需要配置token
  waitTime: 1000 # 缺省值为空
  heartBeatInterval: 10 # 缺省值为10，单位为秒
```

搭建consul服务集群的方式开发者可以[参考](https://learn.hashicorp.com/tutorials/consul/docker-compose-datacenter)。同样地,silky框架提供了使用 docker-compose 部署 **Consul** 集群的编排文件，开发者在获取源码后,进入到 `./samples/docker-compose/infrastr` 目录后,执行如下命令,就可以构建一个 **Consul** 服务集群。

```powershell
docker-compose -f docker-compose.consul.yaml up -d
```