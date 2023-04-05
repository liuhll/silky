---
title: 依赖注入
lang: zh-cn
---

## 什么是依赖注入

把有依赖关系的类放到容器中，在使用时解析出这些类的实例，就是依赖注入。目的是实现类的解耦。

## 将对象注入到ioc容器的方式

### 模块化

1. 在模块中通过`RegisterServices()`方法的`ContainerBuilder`注册服务
  
   开发者如果自定义自定义模块,可以通过模块的定义类注册服务，通过模块定义类向ioc容器注册服务,本质是通过Autofac框架的`ContainerBuilder`实现服务的注册。

   一般地,开发者除非自定义模块,否则不建议通过该方式注册服务。
  
   例如:
  
   ```csharp
     public class MessagePackModule : SilkyModule
     {
         protected override void RegisterServices(ContainerBuilder builder)
         {
            //在此处注入依赖项
             builder.RegisterType<MessagePackTransportMessageDecoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
             builder.RegisterType<MessagePackTransportMessageEncoder>().AsSelf().AsImplementedInterfaces().InstancePerDependency();
         }
     }  
   ```

### 通过ConfigureService实现注册

2. 通过继承`IConfigureService`或是`ISilkyStartup`,通过`Configure()`方法的`IServiceCollection`注册服务
  
   通过继承`IConfigureService`或是`ISilkyStartup`实现服务注册的方式,本质是向.net自带的ioc容器`IServiceCollection`注册服务,在应用启动后,.net自带的ioc容器与autofac的ioc容器会同步更新服务注册信息和服务依赖关系。
   
   一般地,除非与第三方包(框架)整合,例如开发者需要使用[CAP框架](https://github.com/dotnetcore/CAP)或是[MediatR](https://github.com/jbogard/MediatR)实现消息通信,第三方包通过`IServiceCollection`注册相关服务的情况下,开发者可以通过该方式引入第三方的包(框架)的服务注册。
   
   在应用启动时,silky框架会自动扫描继承了`IConfigureService`接口的类型,并通过`Order`属性对所有的实现类进行排序,依次执行`ConfigureServices()`方法,像ioc容器注册相关服务。
   
   例如,如下代码:
  
  ```csharp
  // 需要安装CAP框架相关的包
  public class CapConfigService : IConfigureService
  {
     public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
     { 
        services.AddDbContext<AppDbContext>(); //Options, If you are using EF as the ORM
        services.AddSingleton<IMongoClient>(new MongoClient("")); //Options, If you are using MongoDB
        services.AddCap(x =>
        {
           // If you are using EF, you need to add the configuration：
           x.UseEntityFramework<AppDbContext>(); //Options, Notice: You don't need to config x.UseSqlServer(""") again! CAP can autodiscovery.

           // If you are using ADO.NET, choose to add configuration you needed：
           x.UseSqlServer("Your ConnectionStrings");
           x.UseMySql("Your ConnectionStrings");
           x.UsePostgreSql("Your ConnectionStrings");

           // If you are using MongoDB, you need to add the configuration：
           x.UseMongoDB("Your ConnectionStrings");  //MongoDB 4.0+ cluster

           // CAP support RabbitMQ,Kafka,AzureService as the MQ, choose to add configuration you needed：
           x.UseRabbitMQ("ConnectionString");
           x.UseKafka("ConnectionString");
           x.UseAzureServiceBus("ConnectionString");
           x.UseAmazonSQS();
       });
     }
     public int Order { get; } = 2;
  }
  ```
  
::: warning

对于普通微服务而言,本身底层的rpc通信是通过`dotnetty`框架,使用tcp协议进行通信的。服务于服务之间的通信,并不通过http协议。所以在与第三方框架整合时,普通微服务应用无法使用第三方框架的http中间件,使用http中间件的框架也无法与普通微服务应用整合。

:::

### 依照约定的注册

3. 通过继承依赖注入标识接口实现服务的注册(**推荐**),如果实现这些接口,则会自动将类注册到依赖注入:

- `ITransientDependency` 注册为transient生命周期.
- `ISingletonDependency` 注册为singleton生命周期.
- `IScopedDependency` 注册为scoped生命周期.
   
   开发者一般情况下,如果需要将自定义的一个类注册到ioc容器时,可以选择继承相应的标识接口。例如:自定义的领域服务、仓库等。

   示例:
   
   ```csharp
   public class AccountDomainService : ITransientDependency, IAccountDomainService
   {
   }
   ```
  `AccountDomainService`因为实现了`ITransientDependency`,所以它会自动注册为transient生命周期. 

### 命名的服务注册(InjectNamedAttribute)

在通过继承标识接口实现的依赖注入中,可以通过特性`[InjectNamed("服务名称")]`来实现命名服务的注册;

   示例:
   
   ```csharp
   [InjectNamed("Account")]
   public class AccountDomainService : ITransientDependency, IAccountDomainService
   {
   }
   ```
命名服务无法通过构造注入或是属性注入来获取,只能通过通过服务引擎提供的`ResolveNamed()`方法从Ioc容器中解析对应的命名服务,如下所示:

```csharp

var accountDomainService =  EngineContext.Current.ResolveNamed<IAccountDomainService>("Account");
```


### 固有的注册类型

4. 一些特定类型会默认注册到依赖注入.例子:

- 应用服务代理(被标识了`[ServiceRoute]`的代理接口)被注册为scope.
- 存储库（实现`IRepository`接口）注册为scope.



## 注入依赖关系

使用已注册的服务有三种常用方法.
### 构造方法注入

1. 通过构造注入的方式获取服务对象
     
   通过以上三种方式被注册的的服务都可以通过构造注入的方式获取服务实例对象。
   
   在应用其他微服务应用的应用接口层(包)后,应用服务接口是通过构造注入的方式获取动态代理对象。

   这是将服务注入类的最常用方法.例如:
   
   ```csharp
    public class AccountAppService : IAccountAppService
    {
        private readonly IAccountDomainService _accountDomainService;

        public AccountAppService(IAccountDomainService accountDomainService)
        {
            _accountDomainService = accountDomainService;
        }
    }
   ```

   构造方法注入是将依赖项注入类的首选方式.这样,除非提供了所有构造方法注入的依赖项,否则无法构造类.因此,该类明确的声明了它必需的服务.需要注意的是,

### 属性注入

2. 通过属性注入的方式获取服务对象

   通过继承标识接口注册服务的类,也支持属性注入的方式获取服务对象。如果通过自定义模块类注册服务的时候,也可以指定支持通过属性的方式获取服务。
   
   通过属性的方式获取服务,设置的属性必须为`public`类型的,且必须要有`get`、`set`方法。
   
   一般情况,在获取日志记录类的实例时,为避免在执行测试用例发生空指针异常，可以通过该种方式获取日志记录类的实例。

   例如:
   
   ```csharp
   public class DotNettyRemoteServiceInvoker : IRemoteServiceInvoker
   {

      private readonly ServiceRouteCache _serviceRouteCache;
      private readonly IRemoteServiceSupervisor _remoteServiceSupervisor;
      private readonly ITransportClientFactory _transportClientFactory;
      private readonly IHealthCheck _healthCheck;
      
      public ILogger<DotNettyRemoteServiceInvoker> Logger { get; set; } //可以通过属性注入的方式替换默认的日志记录实例
      
      public DotNettyRemoteServiceInvoker(ServiceRouteCache serviceRouteCache,
            IRemoteServiceSupervisor remoteServiceSupervisor,
            ITransportClientFactory transportClientFactory,
            IHealthCheck healthCheck)
        {
            _serviceRouteCache = serviceRouteCache;
            _remoteServiceSupervisor = remoteServiceSupervisor;
            _transportClientFactory = transportClientFactory;
            _healthCheck = healthCheck;
            Logger = NullLogger<DotNettyRemoteServiceInvoker>.Instance; //在构造器中设置一个默认的日志记录类实例 
        }
   }
   ```

   为了使依赖项成为可选的,我们通常会为依赖项设置默认/后备(fallback)值.在此示例中,`NullLogger`用作后备.因此,如果DI框架或你在创建`DotNettyRemoteServiceInvoker`后未设置Logger属性,则`DotNettyRemoteServiceInvoker`依然可以工作但不写日志.

### 通过服务引擎解析服务

3. 通过服务引擎`IEngine`解析服务实例
   
   silky框架自带的服务引擎提供服务解析方法,可以通过服务引擎解析获取服务实例对象。
   
   例如:
   
   ```csharp
    var accountDomainService = EngineContext.Current.Resolve<IAccountDomainService>();
    ```
