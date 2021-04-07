# 通过lms.samples快速熟悉lms微服务框架的使用

经过一段时间的开发与测试,终于发布了Lms框架的第一个正式版本(1.0.0版本),并给出了lms框架的样例项目**lms.samples**。本文通过对**lms.samples**的介绍，简述如何通过lms框架快速的构建一个微服务的业务框架，并进行应用开发。


## lms.samples项目基本介绍

lms.sample项目由三个独立的微服务应用模块组成:account、stock、order和一个网关项目gateway构成。

### 业务应用模块

每个独立的微服务应用采用模块化设计，主要由如下几部分组成：

1. **主机(Host):** 主要用于托管微服务应用本身，主机通过引用应用服务项目(应用接口的实现),托管微服务应用，通过托管应用服务,在主机启动的过程中,向服务注册中心注册服务路由。

2. **应用接口层(Application.Contracts):** 用于定义应用服务接口,通过应用接口,该微服务模块与其他微服务模块或是网关进行rpc通信的能力。在该项目中,除了定义应用服务接口之前,一般还定义与该应用接口相关的`DTO`对象。应用接口除了被该微服务应用项目引用,并实现应用服务之前,还可以被网关或是其他微服务模块引用。网关或是其他微服务项目通过应用接口生成的代理与该微服务模块通过rpc进行通信。

3. **应用服务层(Application):** 应用服务是该微服务定义的应用接口的实现。应用服务与DDD传统分层架构的应用层的概念一致。主要负责外部通信与领域层之间的协调。一般地，应用服务进行业务流程控制，但是不包含业务逻辑的实现。

4. **领域层(Domain):** 负责表达业务概念,业务状态信息以及业务规则,是该微服务模块的业务核心。一般地,在该层可以定义聚合根、实体、领域服务等对象。

5. **领域共享层(Domain.Shared):** 该层用于定义与领域对象相关的模型、实体等相关类型。不包含任何业务实现，可以被其他微服务引用。

6. **数据访问(DataAccess)层:** 该层一般用于封装数据访问相关的对象。例如：仓库对象、 `SqlHelper`、或是ORM相关的类型等。在lms.samples中,通过efcore实现数据的读写操作。

![project-arch.jpg](/assets/imgs/project-arch.jpg)

### 服务聚合与网关

lms框架不允许服务外部与微服务主机直接通信,应用请求必须通过http请求到达网关,网关通过lms提供的中间件解析到服务条目,并通过rpc与集群内部的微服务进行通信。所以，如果服务需要与集群外部进行通信,那么,开发者定义的网关必须要引用各个微服务模块的应用接口层；以及必须要使用lms相关的中间件。


## 开发环境

1. .net版本: 5.0.101

2. lms版本: 1.0.0

3. IDE: (1) visual studio 最新版 (2) Rider(推荐)

## 主机与应用托管

### 主机的创建步骤

通过lms框架创建一个业务模块非常方便,只需要通过如下4个步骤,就可以轻松的创建一个lms应用业务模块。

1. 创建项目

创建控制台应用(Console Application)项目,并且引用`Silky.Lms.NormHost`包。

```
dotnet add package Silky.Lms.NormHost --version 1.0.0
```

2. 应用程序入口与主机构建

在`main`方法中,通用.net的主机`Host`构建并注册lms微服务。在注册lms微服务时,需要指定lms启动的依赖模块。

一般地,如果开发者不需要额外依赖其他模块,也无需在应用启动或停止时执行方法，那么您可以直接指定`NormHostModule`模块。

```csharp
 public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .RegisterLmsServices<NormHostModule>()
                ;
        }
    }
```

3. 配置文件

lms框架支持`yml`或是`json`格式作为配置文件。通过`appsettings.yml`对lms框架进行统一配置,通过`appsettings.${Environment}.yml`对不同环境变量下的配置项进行设置。

开发者如果直接通过项目的方式启动应用,那么可以通过`Properties/launchSettings.json`的`environmentVariables.DOTNET_ENVIRONMENT`环境变量。如果通过`docker-compose`的方式启动应用,那么可以通过`.env`设置`DOTNET_ENVIRONMENT`环境变量。

为保证配置文件有效,开发者需要显示的将配置文件拷贝到项目生成目录下。

4. 引用应用服务层和数据访问层

一般地,主机项目需要引用该微服务模块的应用服务层和数据访问层。只有主机引用应用服务层,主机在启动时,才会生成服务条目的路由,并且将服务路由注册到服务注册中心。

一个典型的主机项目文件如下所示:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
      <PackageReference Include="Silky.Lms.NormHost" Version="$(LmsVersion)" />
    </ItemGroup>

    <ItemGroup>
      <None Update="appsettings.yml">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
      </None>
      <None Update="appsettings.Production.yml">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
      </None>
      <None Update="appsettings.Development.yml">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
      </None>
    </ItemGroup>

    <ItemGroup>
      <ProjectReference Include="..\Lms.Account.Application\Lms.Account.Application.csproj" />
      <ProjectReference Include="..\Lms.Account.EntityFrameworkCore\Lms.Account.EntityFrameworkCore.csproj" />
    </ItemGroup>
</Project>

```

### 配置

一般地,一个微服务模块的主机必须要配置:服务注册中心、分布式锁链接、分布式缓存地址、集群rpc通信token、数据库链接地址等。

如果使用docker-compose来启动和调试应用的话,那么,rpc配置节点下的的host和port可以缺省,因为生成的每个容器的都有自己的地址和端口号。

如果直接通过项目的方式启动和调试应用的话,那么,必须要配置rpc节点下的port,每个微服务模块的主机应用有自己的端口号。

lms框架的必要配置如下所示:

```yaml
rpc:
  host: 0.0.0.0
  rpcPort: 2201
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
registrycenter:
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 # 使用;来区分不同的服务注册中心
  registryCenterType: Zookeeper
distributedCache:
  redis:
    isEnabled: true 
    configuration: 127.0.0.1:6379,defaultDatabase=0
lock:
  lockRedisConnection: 127.0.0.1:6379,defaultDatabase=1
connectionStrings:
    default: server=127.0.0.1;port=3306;database=account;uid=root;pwd=qwe!P4ss;
```

## 应用接口

## 应用服务--应用接口的实现

## 领域层--微服务的核心业务实现

## 数据访问--通过efcore实现数据读写

## 应用启动与调试