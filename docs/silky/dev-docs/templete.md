---
title: 脚手架
lang: zh-cn
---

## 模板简介

使用 `dotnet new` 命令可以创建[模板](https://docs.microsoft.com/zh-cn/dotnet/core/tools/custom-templates),也就是我们常说的脚手架工具。silky框架提供了两种类型的模板,开发者可以选择合适的模板构建微服务应用。

## 构建独立应用的模板[Silky.App.Template](https://www.nuget.org/packages/Silky.App.Template/)

如果开发者需要独立的开发、管理微服务应用(将微服务应用单独放在一个仓库管理),可以使用[Silky.App.Template](https://www.nuget.org/packages/Silky.App.Template/)模板构建微服务应用。

1. 安装 **Silky.App.Template** 模板

```powershell
dotnet new --install Silky.App.Template::3.0.3
```

2. 创建微服务应用

通过如下命令创建一个新的微服务应用：

```powershell
dotnet new silky.module --hosttype webhost -p:i --includeinfrastr -n Demo
```

**Silky.App.Template** 模板参数:

| 短命令 | 长命令 | 说明  | 缺省值 |
|:-----|:-----|:------|:-----|
| -r|--rpcToken | 设置rpctoken | ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW |
| -rp|--rpcPort | 设置rpc端口 | 2200 |
| -re|--registrycentertype | 服务注册中心类型 | Zookeeper |
| -p:r|--registrycenterconnections | 服务注册中心链接地址 | 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 |
| -p:re|--redisenable | 是否可用redis服务 | true |
| | --redisconfiguration | redis服务配置 |  127.0.0.1:6379,defaultDatabase=0 |
| -d | --dockersupport | 是否支持docker |  true |
| -do | --dotnetenv | 设置运行开发环境 |  Development |
| -in | --includeinfrastr | 是否包含基础服务编排文件 |  true |
|  -p:i | --includesln | 是否包括解决访问文件 |  true |
|  -p:d | --dockernetwork | 设置docker network |  silky_service_net |
|  -ho | --hosttype | 设置主机类型:  webhost, generalhost ,websockethost, gateway |  webhost |

使用 **Silky.App.Template** 模板创建的微服务应用的目录结构:

```
.
├─docker-compose          // docker-compose编排文件
│  ├─Demo                 // Demo微服务应用服务编排
│  └─infrastr             // 基础服务编排文件
│      └─sql
└─src                     // 源代码目录
    ├─Demo.Application    // 应用层
    │  └─System
    ├─Demo.Application.Contracts  // 应用接口层,用于定义服务,可被其他微服务应用引用
    │  └─System
    │      └─Dtos
    ├─Demo.Database.Migrations   // 数据迁移项目(属于基础设施层),用于存放ef迁移文件
    ├─Demo.Domain                // 领域层,实现核心业务应用
    ├─Demo.Domain.Shared         // 领域共享层,用于存放通用的值类型,枚举类型,可被其他微服务应用
    ├─Demo.EntityFrameworkCore   // 数据访问层(属于基础设施层),提供通过efcore提供数据访问能力
    │  └─DbContexts
    └─DemoHost                   // 主机项目,用于应用寄宿,管理应用服务生命周期
        └─Properties             // 应用启动配置
```

3. 启动项目

进入到 **./docker-compose/infrastr** 目录,通过如下命令创建`zookeeper`和`redis`服务:

```powershell
# 创建一个名称为silky_service_net的docker网络
docker network create silky_service_net

# 使用docker-compose创建zookeeper和redis服务
docker-compose -f docker-compose.zookeeper.yml -f docker-compose.redis.yml up -d
```

使用visual studio 或是 rider 打开 Demo.sln 解决方案,将 **DemoHost** 设置为启动项目,还原项目后,按`F5`启动项目。

项目启动后,通过浏览器打开地址 `https://localhost:5001/index.html`, 即可打开swagger在线文档地址:

![templete1.png](/assets/imgs/templete1.png)


## 构建模块化应用的模板[Silky.Module.Template](https://www.nuget.org/packages/Silky.Module.Template)

如果开发者将所有的微服务应用统一开发、管理(将所有微服务应用存放在一个仓库中集中管理)，可以使用[Silky.Module.Template](https://www.nuget.org/packages/Silky.Module.Template)模板构建微服务应用。

1. 安装 **Silky.Module.Template** 模板

```powershell
dotnet new --install Silky.Module.Template::3.0.3
```

2. 创建微服务应用

```powershell
dotnet new silky.module --hosttype webhost -p:i --includeinfrastr --newsln -n Demo
```

**Silky.Module.Template** 模板参数:

| 短命令 | 长命令 | 说明  | 缺省值 |
|:-----|:-----|:------|:-----|
| -r|--rpcToken | 设置rpctoken | ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW |
| -rp|--rpcPort | 设置rpc端口 | 2200 |
| -re|--registrycentertype | 服务注册中心类型 | Zookeeper |
| -p:r|--registrycenterconnections | 服务注册中心链接地址 | 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 |
| -p:re|--redisenable | 是否可用redis服务 | true |
| | --redisconfiguration | redis服务配置 |  127.0.0.1:6379,defaultDatabase=0 |
| -d | --dockersupport | 是否支持docker |  true |
| -do | --dotnetenv | 设置运行开发环境 |  Development |
| -in | --includeinfrastr | 是否包含基础服务编排文件 |  true |
|  -p:i | --includesln | 是否包括解决访问文件 |  true |
|  -p:d | --dockernetwork | 设置docker network |  silky_service_net |
|  -ho | --hosttype | 设置主机类型:  webhost, generalhost ,websockethost, gateway |  webhost |
|  -ne | --newsln|  是否是一个新的解决方案 |  false |

使用 **Silky.Module.Template** 模板创建的微服务应用的目录结构:

```
.
├─docker-compose               // docker-compose编排文件
│  ├─Demo                      // Demo微服务应用服务编排
│  └─infrastr                  // 基础服务编排文件
│      └─sql
└─microservices                // 各个微服务应用模块      
    └─Demo                     // Demo微服务应用
        ├─Demo.Application     // 应用层
        │  └─System
        ├─Demo.Application.Contracts   // 应用接口层,用于定义服务,可被其他微服务应用引用
        │  └─System
        │      └─Dtos
        ├─Demo.Database.Migrations     // 数据迁移项目(属于基础设施层),用于存放ef迁移文件
        ├─Demo.Domain                  // 领域层,实现核心业务应用
        ├─Demo.Domain.Shared           // 领域共享层,用于存放通用的值类型,枚举类型,可被其他微服务应用
        ├─Demo.EntityFrameworkCore     // 数据访问层(属于基础设施层),提供通过efcore提供数据访问能力
        │  └─DbContexts
        └─DemoHost                     // 主机项目,用于应用寄宿,管理应用服务生命周期
            └─Properties               // 应用启动配置
```

3. 新增一个微服务应用模块

```powershell
dotnet new silky.module --hosttype webhost -p:i -n Demo1
```

将新创建的微服务应用从 **Demo1/microservices/Demo1** 拷贝到 **Demo/microservices/Demo1**,  **Demo1/docker-compose/Demo1** 拷贝到 **Demo/docker-compose/Demo1**,并将新模块的微服务应用添加到解决方案中。

![templete2.png](/assets/imgs/templete2.png)

4. 调式

如果开发者需要同时启动调式多个微服务,需要更新`rpc:port`配置(`rpc:port`不能重复),并通过更新`launchSettings.json`更新应用服务启动的http服务地址。将启动项目设置为 **多个启动项目**,将 **Demo1Host** 和 **DemoHost** 设置为 **启动**。这样, 我们就可以同时调式 **Demo1Host** 和 **DemoHost** 这两个应用了。

![templete3.png](/assets/imgs/templete3.png)

![templete4.png](/assets/imgs/templete4.png)

![templete5.png](/assets/imgs/templete5.png)