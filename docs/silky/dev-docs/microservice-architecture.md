---
title: 微服务模块化架构的最佳实践 & 约定
lang: zh-cn
---

## 解决方案结构

- **推荐** 为每个微服务应用模块创建一个单独的解决方案。
- **推荐** 将解决方案命名为*CompanyName.MicroServiceName*(对于Silky核心模块来说,它的命名方式是*Silky.ModuleName*)。
- **推荐** 一个每个微服务应用做为分层项目开发,因为它有几个包(项目)是相互关联的。

## 层(layers) & 包(packages)

### 业务微服务应用模块的分层

下图展示了一个普通微服务应用模块的包以及他们之间的依赖关系:

![silky-project.png](/assets/imgs/silky-project.png)

最终的目的是让应用程序以灵活的方式进行开发和被其他微服务应用模块引用。实例应用程序。

#### 领域层

- **推荐** 将领域层划分为两个项目:
   - **Domain.Shared** 包(项目) 命名为*CompanyName.MicroServiceName.Domain.Shared*,包含常量,枚举和其他类型, 它不能包含实体,存储库,域服务或任何其他业务对象. 可以安全地与模块中的所有层使用. 此包也可以与第三方客户端使用。
   - **Domain** 包(项目) 命名为*CompanyName.MicroServiceName.Domain*, 包含实体, 仓储接口,领域服务接口及其实现和其他领域对象。
      - **Domain** 包依赖于 *Domain.Shared* 包。
      - **Domain** 包可以依赖自身微服务应用模块的 *Application.Contracts* 包。
      - **Domain** 包可以根据实际的业务关系,通过项目引用或是nuget包的方式依赖其他微服务模块的 *Application.Contracts* 包。

#### 应用服务层
- **推荐** 将应用服务层划分为两个项目:
   - **Application.Contracts** 包(项目) 命名为*CompanyName.MicroServiceName.Application.Contracts*,包含应用服务接口和相关的数据传输对象(DTO)。
      - **Application contract** 包依赖于 *Domain.Shared* 包。
   - **Application** 包(项目)命名为*CompanyName.MicroServiceName.Application*,包含应用服务实现。
      - **Application** 包依赖于 *Domain* 包和 *Application.Contracts* 包。

#### 基础设施层
- **推荐** 为每个orm/数据库集成创建一个独立的集成包, 比如Entity Framework Core 和 MongoDB。
  - **推荐** 例如, 创建一个抽象Entity Framework Core集成的CompanyName.MicroServiceName.EntityFrameworkCore 包。 ORM 集成包依赖于 *Domain* 包。
  - **不推荐** 依赖于orm/数据库集成包中的其他层。
  - **推荐** 如果使用code first方式进行开发,推荐为项目迁移简历一个单独的项目(包)。
- **推荐** 为每个主要的库创建一个独立的集成包, 在不影响其他包的情况下可以被另一个库替换。

#### Host层
- **推荐** 创建命名为**CompanyName.MicroServiceNameHost**的 **Host** 包。用于托管微服务应用本身。
- **必须** 依赖 **Application** 包(项目) 只有依赖了**Application** 包,服务应用方法(服务条目)才会生成相应的路由信息,并注册到服务注册中心。

### 网关应用模块的分层

对网关应用而言,并不需要实现具体的业务代码,但是可以在网关应用自定义或是引入第三方框架的中间件。网关应用需要依赖其他微服务应用模块的 **Application.Contracts** 包(项目)。

下图展示了网关应用的依赖关系：

![silky-gateway-project.png](/assets/imgs/silky-gateway-project.png)

#### Host层
- **推荐** 创建命名为**CompanyName.MicroServiceNameGatewayHost**的 **Host** 包。用于托管网关应用本身。
- **必须** 网关项目需要通过依赖其他微服务应用模块的**Application Contract** 包。
- **推荐** 如果开发者需要自定义中间件,开发将其封装为一个单独的包。
- **推荐** 在网关应用也可以引用第三方中间件,对http请求做统一处理。