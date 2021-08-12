---
title: 通过silky.samples熟悉silky微服务框架的使用
lang: zh-cn
---


经过一段时间的开发与测试,终于发布了Silky框架的第一个正式版本(1.0.0版本),并给出了silky框架的样例项目**silky.samples**。本文通过对**silky.samples**的介绍，简述如何通过silky框架快速的构建一个微服务的业务框架，并进行应用开发。


## silky.samples项目基本介绍

silky.sample项目由三个独立的微服务应用模块组成:account、stock、order和一个网关项目gateway构成。

### 业务应用模块

每个独立的微服务应用采用模块化设计，主要由如下几部分组成：

1. **主机(Host):** 主要用于托管微服务应用本身，主机通过引用应用服务项目(应用接口的实现),托管微服务应用，通过托管应用服务,在主机启动的过程中,向服务注册中心注册服务路由。

2. **应用接口层(Application.Contracts):** 用于定义应用服务接口,通过应用接口,该微服务模块与其他微服务模块或是网关进行rpc通信的能力。在该项目中,除了定义应用服务接口之前,一般还定义与该应用接口相关的`DTO`对象。应用接口除了被该微服务应用项目引用,并实现应用服务之前,还可以被网关或是其他微服务模块引用。网关或是其他微服务项目通过应用接口生成的代理与该微服务模块通过rpc进行通信。

3. **应用服务层(Application):** 应用服务是该微服务定义的应用接口的实现。应用服务与DDD传统分层架构的应用层的概念一致。主要负责外部通信与领域层之间的协调。一般地，应用服务进行业务流程控制，但是不包含业务逻辑的实现。

4. **领域层(Domain):** 负责表达业务概念,业务状态信息以及业务规则,是该微服务模块的业务核心。一般地,在该层可以定义聚合根、实体、领域服务等对象。

5. **领域共享层(Domain.Shared):** 该层用于定义与领域对象相关的模型、实体等相关类型。不包含任何业务实现，可以被其他微服务引用。

6. **数据访问(DataAccess)层:** 该层一般用于封装数据访问相关的对象。例如：仓库对象、 `SqlHelper`、或是ORM相关的类型等。在silky.samples中,通过efcore实现数据的读写操作。

![project-arch.jpg](/assets/imgs/project-arch.jpg)

### 服务聚合与网关

silky框架不允许服务外部与微服务主机直接通信,应用请求必须通过http请求到达网关,网关通过silky提供的中间件解析到服务条目,并通过rpc与集群内部的微服务进行通信。所以，如果服务需要与集群外部进行通信,那么,开发者定义的网关必须要引用各个微服务模块的应用接口层；以及必须要使用silky相关的中间件。


## 开发环境

1. .net版本: 5.0.101

2. silky版本: 1.0.0

3. IDE: (1) visual studio 最新版 (2) Rider(推荐)

## 主机与应用托管

### 主机的创建步骤

通过silky框架创建一个业务模块非常方便,只需要通过如下4个步骤,就可以轻松的创建一个silky应用业务模块。

1. 创建项目

创建控制台应用(Console Application)项目,并且引用`Silky.NormHost`包。

```
dotnet add package Silky.NormHost --version 1.0.0
```

2. 应用程序入口与主机构建

在`main`方法中,通用.net的主机`Host`构建并注册silky微服务。在注册silky微服务时,需要指定silky启动的依赖模块。

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
                    .RegisterSilkyServices<NormHostModule>()
                ;
        }
    }
```

3. 配置文件

silky框架支持`yml`或是`json`格式作为配置文件。通过`appsettings.yml`对silky框架进行统一配置,通过`appsettings.${Environment}.yml`对不同环境变量下的配置项进行设置。

开发者如果直接通过项目的方式启动应用,那么可以通过`Properties/launchSettings.json`的`environmentVariables.DOTNET_ENVIRONMENT`环境变量。如果通过`docker-compose`的方式启动应用,那么可以通过`.env`设置`DOTNET_ENVIRONMENT`环境变量。

为保证配置文件有效,开发者需要显式的将配置文件拷贝到项目生成目录下。

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
      <PackageReference Include="Silky.NormHost" Version="$(SilkyVersion)" />
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
      <ProjectReference Include="..\Silky.Account.Application\Silky.Account.Application.csproj" />
      <ProjectReference Include="..\Silky.Account.EntityFrameworkCore\Silky.Account.EntityFrameworkCore.csproj" />
    </ItemGroup>
</Project>

```

### 配置

一般地,一个微服务模块的主机必须要配置:服务注册中心、分布式锁链接、分布式缓存地址、集群rpc通信token、数据库链接地址等。

如果使用docker-compose来启动和调试应用的话,那么,rpc配置节点下的的host和port可以缺省,因为生成的每个容器的都有自己的地址和端口号。

如果直接通过项目的方式启动和调试应用的话,那么,必须要配置rpc节点下的port,每个微服务模块的主机应用有自己的端口号。

silky框架的必要配置如下所示:

```yaml
rpc:
  host: 0.0.0.0
  Port: 2201
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
registrycenter:
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186 # 使用分号;来区分不同的服务注册中心
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

### 应用接口定义

一般地,在应用接口层开发者需要安装`Silky.Rpc`包。如果该微服务模块还涉及到分布式事务,那么还需要安装`Silky.Transaction.Tcc`,当然，您也可以选择在应用接口层安装`Silky.Transaction`包,在应用服务层安装`Silky.Transaction.Tcc`包。

1. 开发者只需要在应用接口通过`ServiceRouteAttribute`特性对应用接口进行直接即可。

2. Silky约定应用接口应当以`IXxxAppService`命名，这样,服务条目生成的路由则会以`api/xxx`形式生成。当然这并不是强制的。

3. 每个应用接口的方法都对应着一个服务条目,服务条目的Id为: 方法的完全限定名 + 参数名

4. 您可以在应用接口层对方法的缓存、路由、服务治理、分布式事务进行相关配置。该部分内容请参考[官方文档](http://docs.silky-fk.com/)

5. 网关或是其他模块的微服务项目需要引用服务应用接口项目或是通过nuget的方式安装服务应用接口生成的包。

6. `[Governance(ProhibitExtranet = true)]`可以标识一个方法禁止与集群外部进行通信,通过网关也不会生成swagger文档。
 
7. 应用接口方法生成的WebApi支持restful API风格。Silky支持通过方法的约定命名生成对应http方法请求的WebApi。您当然开发者也可以通过`HttpMethodAttribute`特性对某个方法进行注解。

### 一个典型的应用接口的定义

```csharp
    /// <summary>
    /// 账号服务
    /// </summary>
    [ServiceRoute]
    public interface IAccountAppService
    {
        /// <summary>
        /// 新增账号
        /// </summary>
        /// <param name="input">账号信息</param>
        /// <returns></returns>
        Task<GetAccountOutput> Create(CreateAccountInput input);

        /// <summary>
        /// 通过账号名称获取账号
        /// </summary>
        /// <param name="name">账号名称</param>
        /// <returns></returns>
        [GetCachingIntercept("Account:Name:{0}")]
        [HttpGet("{name:string}")]
        Task<GetAccountOutput> GetAccountByName([CacheKey(0)] string name);

        /// <summary>
        /// 通过Id获取账号信息
        /// </summary>
        /// <param name="id">账号Id</param>
        /// <returns></returns>
        [GetCachingIntercept("Account:Id:{0}")]
        [HttpGet("{id:long}")]
        Task<GetAccountOutput> GetAccountById([CacheKey(0)] long id);

        /// <summary>
        /// 更新账号信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [UpdateCachingIntercept( "Account:Id:{0}")]
        Task<GetAccountOutput> Update(UpdateAccountInput input);

        /// <summary>
        /// 删除账号信息
        /// </summary>
        /// <param name="id">账号Id</param>
        /// <returns></returns>
        [RemoveCachingIntercept("GetAccountOutput","Account:Id:{0}")]
        [HttpDelete("{id:long}")]
        Task Delete([CacheKey(0)]long id);

        /// <summary>
        /// 订单扣款
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Governance(ProhibitExtranet = true)]
        [RemoveCachingIntercept("GetAccountOutput","Account:Id:{0}")]
        [Transaction]
        Task<long?> DeductBalance(DeductBalanceInput input);
    }
```


## 应用服务--应用接口的实现

1. 应用服务层只需要引用应用服务接口层以及领域服务层,并实现应用接口相关的方法。

2. 确保该微服务模块的主机引用了该模块的应用服务层,这样主机才能够托管该应用本身。

3. 应用服务层可以通过引用其他微服务模块的应用接口层项目(或是安装nuget包,取决于开发团队的项目管理方法),与其他微服务模块进行rpc通信。

4. 应用服务层需要依赖领域服务,通过调用领域服务的相关接口,实现该模块的核心业务逻辑。

5. DTO到实体对象或是实体对DTO对象的映射关系可以在该层指定映射关系。

一个典型的应用服务的实现如下所示:

```csharp
public class AccountAppService : IAccountAppService
    {
        private readonly IAccountDomainService _accountDomainService;

        public AccountAppService(IAccountDomainService accountDomainService)
        {
            _accountDomainService = accountDomainService;
        }

        public async Task<GetAccountOutput> Create(CreateAccountInput input)
        {
            var account = input.MapTo<Domain.Accounts.Account>();
            account = await _accountDomainService.Create(account);
            return account.MapTo<GetAccountOutput>();
        }

        public async Task<GetAccountOutput> GetAccountByName(string name)
        {
            var account = await _accountDomainService.GetAccountByName(name);
            return account.MapTo<GetAccountOutput>();
        }

        public async Task<GetAccountOutput> GetAccountById(long id)
        {
            var account = await _accountDomainService.GetAccountById(id);
            return account.MapTo<GetAccountOutput>();
        }

        public async Task<GetAccountOutput> Update(UpdateAccountInput input)
        {
            var account = await _accountDomainService.Update(input);
            return account.MapTo<GetAccountOutput>();
        }

        public Task Delete(long id)
        {
            return _accountDomainService.Delete(id);
        }

        [TccTransaction(ConfirmMethod = "DeductBalanceConfirm", CancelMethod = "DeductBalanceCancel")]
        public async Task<long?> DeductBalance(DeductBalanceInput input)
        {
            var account = await _accountDomainService.GetAccountById(input.AccountId);
            if (input.OrderBalance > account.Balance)
            {
                throw new BusinessException("账号余额不足");
            }
            return await _accountDomainService.DeductBalance(input, TccMethodType.Try);
        }

        public Task DeductBalanceConfirm(DeductBalanceInput input)
        {
            return _accountDomainService.DeductBalance(input, TccMethodType.Confirm);
        }

        public Task DeductBalanceCancel(DeductBalanceInput input)
        {
            return _accountDomainService.DeductBalance(input, TccMethodType.Cancel);
        }
    }
```

## 领域层--微服务的核心业务实现

1. 领域层是该微服务模块核心业务处理的模块,一般用于定于聚合根、实体、领域服务、仓储等业务对象。

2. 领域层引用该微服务模块的应用接口层,方便使用dto对象。

3. 领域层可以通过引用其他微服务模块的应用接口层项目(或是安装nuget包,取决于开发团队的项目管理方法),与其他微服务模块进行rpc通信。

4. 领域服务必须要直接或间接继承`ITransientDependency`接口,这样,该领域服务才会被注入到ioc容器。

6. silky.samples 项目使用[TanvirArjel.EFCore.GenericRepository](https://github.com/TanvirArjel/EFCore.GenericRepository)包实现数据的读写操作。

一个典型的领域服务的实现如下所示:

```csharp
  public class AccountDomainService : IAccountDomainService
    {
        private readonly IRepository _repository;
        private readonly IDistributedCache<GetAccountOutput, string> _accountCache;

        public AccountDomainService(IRepository repository,
            IDistributedCache<GetAccountOutput, string> accountCache)
        {
            _repository = repository;
            _accountCache = accountCache;
        }

        public async Task<Account> Create(Account account)
        {
            var exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Name == account.Name);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Name}名称的账号");
            }

            exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Email == account.Email);
            if (exsitAccountCount > 0)
            {
                throw new BusinessException($"已经存在{account.Email}Email的账号");
            }

            await _repository.InsertAsync<Account>(account);
            return account;
        }

        public async Task<Account> GetAccountByName(string name)
        {
            var accountEntry = _repository.GetQueryable<Account>().FirstOrDefault(p => p.Name == name);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在名称为{name}的账号");
            }

            return accountEntry;
        }

        public async Task<Account> GetAccountById(long id)
        {
            var accountEntry = _repository.GetQueryable<Account>().FirstOrDefault(p => p.Id == id);
            if (accountEntry == null)
            {
                throw new BusinessException($"不存在Id为{id}的账号");
            }

            return accountEntry;
        }

        public async Task<Account> Update(UpdateAccountInput input)
        {
            var account = await GetAccountById(input.Id);
            if (!account.Email.Equals(input.Email))
            {
                var exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Email == input.Email);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Email为{input.Email}的账号");
                }
            }

            if (!account.Name.Equals(input.Name))
            {
                var exsitAccountCount = await _repository.GetCountAsync<Account>(p => p.Name == input.Name);
                if (exsitAccountCount > 0)
                {
                    throw new BusinessException($"系统中已经存在Name为{input.Name}的账号");
                }
            }

            await _accountCache.RemoveAsync($"Account:Name:{account.Name}");
            account = input.MapTo(account);
            await _repository.UpdateAsync(account);
            return account;
        }

        public async Task Delete(long id)
        {
            var account = await GetAccountById(id);
            await _accountCache.RemoveAsync($"Account:Name:{account.Name}");
            await _repository.DeleteAsync(account);
        }

        public async Task<long?> DeductBalance(DeductBalanceInput input, TccMethodType tccMethodType)
        {
            var account = await GetAccountById(input.AccountId);
            var trans = await _repository.BeginTransactionAsync();
            BalanceRecord balanceRecord = null;
            switch (tccMethodType)
            {
                case TccMethodType.Try:
                    account.Balance -= input.OrderBalance;
                    account.LockBalance += input.OrderBalance;
                    balanceRecord = new BalanceRecord()
                    {
                        OrderBalance = input.OrderBalance,
                        OrderId = input.OrderId,
                        PayStatus = PayStatus.NoPay
                    };
                    await _repository.InsertAsync(balanceRecord);
                    RpcContext.GetContext().SetAttachment("balanceRecordId",balanceRecord.Id);
                    break;
                case TccMethodType.Confirm:
                    account.LockBalance -= input.OrderBalance;
                    var balanceRecordId1 = RpcContext.GetContext().GetAttachment("orderBalanceId")?.To<long>();
                    if (balanceRecordId1.HasValue)
                    {
                        balanceRecord = await _repository.GetByIdAsync<BalanceRecord>(balanceRecordId1.Value);
                        balanceRecord.PayStatus = PayStatus.Payed;
                        await _repository.UpdateAsync(balanceRecord);
                    }
                    break;
                case TccMethodType.Cancel:
                    account.Balance += input.OrderBalance;
                    account.LockBalance -= input.OrderBalance;
                    var balanceRecordId2 = RpcContext.GetContext().GetAttachment("orderBalanceId")?.To<long>();
                    if (balanceRecordId2.HasValue)
                    {
                        balanceRecord = await _repository.GetByIdAsync<BalanceRecord>(balanceRecordId2.Value);
                        balanceRecord.PayStatus = PayStatus.Cancel;
                        await _repository.UpdateAsync(balanceRecord);
                    }
                    break;
            }

           
            await _repository.UpdateAsync(account);
            await trans.CommitAsync();
            await _accountCache.RemoveAsync($"Account:Name:{account.Name}");
            return balanceRecord?.Id;
        }
    }
```

## 数据访问(EntityFrameworkCore)--通过efcore实现数据读写

1. silky.samples项目使用orm框架efcore进行数据读写。

2. silky提供了`IConfigureService`,通过继承该接口即可使用`IServiceCollection`的实例指定数据上下文对象和注册仓库服务。

```csharp
  public class EfCoreConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OrderDbContext>(opt =>
                    opt.UseMySql(configuration.GetConnectionString("Default"),
                        ServerVersion.AutoDetect(configuration.GetConnectionString("Default"))))
                .AddGenericRepository<OrderDbContext>(ServiceLifetime.Transient)
                ;
        }

        public int Order { get; } = 1;
    }
```

3. 主机项目需要显式的引用该项目，只有这样,该项目的`ConfigureServices`才会被调用。

4. 数据迁移,请[参考](https://docs.microsoft.com/zh-cn/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli)

## 应用启动与调试

### 获取源码

1. 使用git 克隆silky项目源代码,silky.samples存放在`samples`目录下

```cmd
# github
git clone https://github.com/liuhll/silky.git

# gitee
git clone https://gitee.com/liuhll2/silky.git
```

### 必要的前提

1. 服务注册中心`zookeeper`

2. 缓存服务`redis`

3. mysql数据库 

如果您电脑已经安装了[docker](https://docs.docker.com/docker-for-windows/install)以及[docker-compose](https://docs.docker.com/compose/install/)命令,那么您只需要进入`samples\docker-compose\infrastr`目录下,打开命令行工作,执行如下命令就可以自动安装`zookeeper`、`redis`、`mysql`等服务:

```cmd
docker-compose -f .\docker-compose.mysql.yml -f .\docker-compose.redis.yml -f .\docker-compose.zookeeper.yml up -d
```

### 数据库迁移

需要分别进入到各个微服务模块下的`EntityFrameworkCore`项目(例如:),执行如下命令:

```cmd
dotnet ef database update
```

例如: 需要迁移account模块的数据库如下所示:

![db-migrations.png](/assets/imgs/db-migrations.png)

order模块和stock模块与account模块一致,在服务运行前都需要通过数据库迁移命令生成相关数据库。

::: warning 注意

1. 数据库迁移指定数据库连接地址默认指定的是`appsettings.Development.yml`中配置的,您可以通过修改该配置文件中的`connectionStrings.default`配置项来指定自己的数据库服务地址。

2. 如果没有`dotnet ef`命令,则需要通过`dotnet tool install --global dotnet-ef`安装ef工具,请[参考](https://docs.microsoft.com/zh-cn/ef/core/get-started/overview/install)


:::

### 以项目的方式启动和调试

#### 使用visual studio作为开发工具

进入到samples目录下,使用visual studio打开`silky.samples.sln`解决方案,将项目设置为多启动项目,并将网关和各个模块的微服务主机设置为启动项目，如下图:

![visual-studio-debug-1](/assets/imgs/visual-studio-debug-1.png)

设置完成后直接启动即可。

#### 使用rider作为开发工具

1. 进入到samples目录下,使用rider打开`silky.samples.sln`解决方案,打开各个微服务模块下的`Properties/launchSettings.json`,点击图中绿色的箭头即可启动项目。

![rider-debug.png](/assets/imgs/rider-debug.png)

2. 启动网关项目后,可以看到应用接口的服务条目生成的webapi接口。

![swagger-ui.png](/assets/imgs/swagger-ui.png)

::: warning 注意

1. 默认的环境变量为: `Development`,如果需要修改环境变量的话,可以通过`Properties/launchSettings.json`下的`environmentVariables`节点修改相关环境变量,请参考[在 ASP.NET Core 中使用多个环境](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/environments?view=aspnetcore-5.0)。

2. 数据库连接、服务注册中心地址、以及redis缓存地址和分布式锁连接等配置项可以通过修改`appsettings.Development.yml`配置项自定义指定。

:::

### 以docker-compose的方式启动和调试

1. 进入到samples目录下,使用visual studio打开`silky.samples.dockercompose.sln`解决方案,将**docker-compose**设置为启动项目，即可启动和调式。

2. 应用启动成功后,打开: [http://127.0.0.1/swagger](http://127.0.0.1/swagger),即可看到swagger api文档

![swagger-ui2.png](/assets/imgs/swagger-ui2.png)


::: warning 注意

1. 以docker-compose的方式启动和调试,则指定的环境变量为:`ContainerDev`

2. 数据库连接、服务注册中心地址、以及redis缓存地址和分布式锁连接等配置项可以通过修改`appsettings.ContainerDev.yml`配置项自定义指定,配置的服务连接地址**不允许**为: `127.0.0.1`或是`localhost`

:::


### 测试和调式

服务启动成功后,您可以通过`/api/account-post`接口和`/api/product-post`接口新增账号和产品,然后通过`/api/order-post`接口进行测试和调式。

## 开源地址

github: [https://github.com/liuhll/silky](https://github.com/liuhll/silky)

gitee: [https://gitee.com/liuhll2/silky](https://gitee.com/liuhll2/silky)
