---
title: 模块
lang: zh-cn
---

## 模块的定义

Silky是一个包括多个nuget包构成的模块化的框架,每个模块将程序划分为一个个小的结构,在这个结构中有着自己的逻辑代码和自己的作用域，不会影响到其他的结构。

### 模块类

一般地，一个模块的定义是通过在该程序集内创建一个派生自 `SilkyModule`的类,如下所示:

```csharp

public class RpcModule : SilkyModule
{

}

```

[SilkyModule](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/SilkyModule.cs)是一个抽象的类,它定义了模块的基础方法，体现了模块在框架中的作用;

`SilkyModule`模块定义的核心代码如下所示:

```csharp
public abstract class SilkyModule : Autofac.Module, ISilkyModule, IDisposable
{

   protected SilkyModule()
   {
       Name = GetType().Name.RemovePostFix(StringComparison.OrdinalIgnoreCase, "Module");
   }

   protected override void Load([NotNull] ContainerBuilder builder)
   {
       base.Load(builder);
       RegisterServices(builder);
   }

    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
 
    protected virtual void RegisterServices([NotNull] ContainerBuilder builder)
    {
    }

    public virtual Task Initialize([NotNull] ApplicationContext applicationContext)
    {
       return Task.CompletedTask;
    }

    public virtual Task Shutdown([NotNull] ApplicationContext applicationContext)
    {
        return Task.CompletedTask;
    }

    public virtual string Name { get; }

   // 其他代码略...
}
```

通过对`SilkyModule`模块代码定义的分析我们可以得知,一个Silky模块有如下几个作用:

1. 在`ConfigureServices()`方法中,通过`IServiceCollection`实现服务注册;

2. 在`RegisterServices()`方法中,通过`ContainerBuilder`实现服务注册;

3. 在应用程序启动时,通过`Initialize()`方法实现模块的初始化方法; 

4. 在应用程序停止时,执行`Shutdown()`方法,可以实现模块资源的释放;


关于上述第1、2 点的作用, 我们已经在[服务引擎一章](/source/startup/engine.html#服务引擎的作用)中做了详细的解析;关于第3、4点的作用,应用程序是如何在启动时调用`Initialize()`方法或是在停止时执行`Shutdown()`方法呢?

在[构建服务引擎](/source/startup/engine.html#构建服务引擎)一章中,我们提到,在构建服务引擎时,我们有一项很重要的工作就是注册了[InitSilkyHostedService](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/InitSilkyHostedService.cs)[后台任务](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio)。

后台任务`InitSilkyHostedService`的源码如下所示:

```csharp
    public class InitSilkyHostedService : IHostedService
    {
        private readonly IModuleManager _moduleManager;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public InitSilkyHostedService(IServiceProvider serviceProvider,
            IModuleManager moduleManager,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            if (EngineContext.Current is SilkyEngine)
            {
                EngineContext.Current.ServiceProvider = serviceProvider;
            }

            _moduleManager = moduleManager;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine(@"                                              
   _____  _  _  _           
  / ____|(_)| || |          
 | (___   _ | || | __ _   _ 
  \___ \ | || || |/ /| | | |
  ____) || || ||   < | |_| |
 |_____/ |_||_||_|\_\ \__, |
                       __/ |
                      |___/
            ");
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var ver = $"{version.Major}.{version.Minor}.{version.Build}";
            Console.WriteLine($" :: Silky ::        {ver}");
            _hostApplicationLifetime.ApplicationStarted.Register(async () =>
            {
                await _moduleManager.InitializeModules();
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStopped.Register(async () =>
            {
                await _moduleManager.ShutdownModules();
            });
        }
    }
```

1. 在后台任务`StartAsync()`,在打印Silky的banner后,在应用启动时注册一个回调方法,通过模块管理器`IModuleManager`执行初始化模块方法;

2. 在后台任务`StopAsync()`,在应用停止后注册一个回调方法,通过模块管理器`IModuleManager`执行关闭模块方法,一般用于各个模块的资源释放;

下面,我们查看模块管理器[ModuleManager](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/ModuleManager.cs)是如何初始化模块的:

```csharp
        public async Task InitializeModules()
        {
            foreach (var module in _moduleContainer.Modules)
            {
                try
                {
                    Logger.LogInformation("Initialize the module {0}", module.Name);
                    await module.Instance.Initialize(new ApplicationContext(_serviceProvider, _moduleContainer));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Initializing the {module.Name} module is an error, reason: {e.Message}");
                    throw;
                }
            }
        }
```

模块容器`_moduleContainer`的属性`_moduleContainer.Modules`是通过模块加载器`ModuleLoader`加载并通过依赖关系进行排序得到的所有模块的实例,我们看到通过`foreach`对所有的模块实例进行遍历,并依次执行各个模块的`Initialize()`方法。

同样的，在应用程序停止时,会调用`InitSilkyHostedService`任务的`StopAsync()`,该方法通过调用模块管理器的`ShutdownModules()`方法,实现对各个模块资源的释放;

```csharp
    public async Task ShutdownModules()
    {
        foreach (var module in _moduleContainer.Modules)
        {
            await module.Instance.Shutdown(new ApplicationContext(_serviceProvider, _moduleContainer));
        }
    }
```

## 模块的类型

在Silky框架中,我将模块的类型划分为如下几种类型:

1. 模块的定义`SilkyModule`: [SilkyModule](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/SilkyModule.cs)是一个抽象的模块,用于定义模块的概念;其他业务模块必须要派生自该类;

2. 业务模块: 直接派生自`SilkyModule`类的非抽象类,Silky框架中,几乎所有的包在通过定义业务模块后从而实现模块化编程的,很多核心的包都是业务模块,如:`SilkyModule`、`ConsulModule`、`DotNettyModule`等等模块都属于业务模块;

3. Http类型的业务模块:该类型的业务模块派生自[HttpSilkyModule](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/HttpSilkyModule.cs),相比一般的业务模块,该类型的模块增加了`Configure(IApplicationBuilder application)`方法,该类型的模块一般用于通过web主机构建的微服务应用或是网关中,可以在`Configure()`方法中通过`IApplicationBuilder`引用http中间件,在silky框架中,诸如: `CorsModule`、`IdentityModule`、`MiniProfilerModule`等均是该类型的模块; 需要特别注意的是，需要http业务模块配置的中间件起效果的话，不要忘记需要在`Startup`类中的`Configure`进行如下配置：

```csharp

public void Configure(IApplicationBuilder app, IWebHostEnvironment env) 
{
   app.ConfigureSilkyRequestPipeline();
}
```

4. 启动模块:该类型的模块派生自[StartUpModule](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/StartUpModule.cs)的非抽象类;在模块加载过程中,通过指定启动模块,从而得知模块的依赖关系,模块加载器会通过模块的依赖对模块进行排序,从而影响应用在启动时各个模块的执行的先后顺序;Silky模块预定义了多个启动模块,可以用于不同silky主机的构成:
  A) `DefaultGeneralHostModule` 用于构建普通的业务主机,一般用于托管只提供RPC服务的微服务应用;
  B) `WebSocketHostModule` 用于构建提供WebSocket服务能力的业务主机;
  C) `DefaultWebHostModule` 用于构建能够提供Http服务的业务主机,对外可以提供http服务,也可以用于内部rpc通信;
  D) `DefaultGatewayHostModule` 用于构建网关微服务,一般为微服务集群暴露对外部的http访问端口,通过路由机制,将http请求转发到具体某个服务条目,对内通过RPC进行通信;

除此之外,开发者也可以自己的需求,为自己定义需要的启动模块,在构建微服务主机时,指定相应的启动模块。


## 模块的加载

Silky所有的模块是在什么时候以及如何进行加载和排序的呢?

在之前的[构建服务引擎](/source/startup/engine.html#构建服务引擎)的一章中,我们知道在`AddSilkyServices<T>()`方法中,我们通过泛型`T`来指定应用程序的启用模块`StartUpModule`类型。并构建了模块加载器对象`ModuleLoader`,并且将模块加载器对象作为服务引擎的`LoadModules()`方法参数:

```csharp
public static IEngine AddSilkyServices<T>(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment) where T : StartUpModule
{
    var moduleLoader = new ModuleLoader();
    engine.LoadModules<T>(services, moduleLoader);
}
```

在服务引擎`SilkyEngine`实现类中,除了实现`IEngine`接口之外,还需要实现了`IModuleContainer`接口,`IModuleContainer`只定义了一个只读属性`Modules`，要求通过该属性获取所有的模块;在服务引擎中,我们通过模块加载器对象`moduleLoader.LoadModules()`方法实现对模块的加载与解析,并对属性`Modules`进行赋值;

```csharp
internal sealed class SilkyEngine : IEngine, IModuleContainer
{
  // 其他代码略...

  
   public void LoadModules<T>(IServiceCollection services, IModuleLoader moduleLoader)
   where T : StartUpModule
   {
      Modules = moduleLoader.LoadModules(services, typeof(T));
   }
  
   // 实现IModuleContainer定义的属性
   public IReadOnlyList<ISilkyModuleDescriptor> Modules { get; private set; }
}
```

模块加载器[ModuleLoader](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/ModuleLoader.cs)要求传递两个参数,一个是`IServiceCollection`的对象`services`，一个是启动模块`StartupModule`的的类型`typeof(T)`;下面我们来描述模块加载的过程:

1. 通过`SilkyModuleHelper.FindAllModuleTypes(startupModuleType)` 查找到启动模块`StartupModule`类型依赖的所有模块类型;

2. 通过反射创建模块的实例,并通过`IServiceCollection`注册单例的模块实例,并创建模块描述符`SilkyModuleDescriptor`;

3. 根据模块的依赖关系对模块进行排序;

模块的依赖关系是通过特性`DependsOnAttribute`指定的,通过[DependsOnAttribute](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/DependsOnAttribute.cs)在对模块的类进行标注,就可以解析到各个模块的依赖关系,从而实现通过模块的依赖关系进行排序;

::: tip 提示

熟悉[APB框架](https://github.com/abpframework/abp)的小伙伴应该可以看出来,Silky模块的设计主要是借鉴了APB框架的模块设计,在一些细节方面做了调整。

::: 

## Silky的核心模块

通过上面的介绍, 我们知道一个模块类的最重要的工作主要由两点: 1. 实现服务的注册; 2. 在应用启动时或是停止时执行指定的方法完成初始化任务或是释放资源的任务;

如何判断是否是silky的核心模块呢? 核心模块最重要的一个作用就是在应用启动时,通过`Initialize()`方法执行该模块的初始化资源的任务;

通过查看源码，我们发现大部分silky模块在应用启动时并没有重写`Initialize()`方法,也就是说,大部分silky模块在应用启动过程时主要是完成各个模块的服务类的注册并不需要做什么工作。

![SilkyModel.png](/assets/imgs/SilkyModel.png)

如上图所示,我们看到silky框架定义的模块,由如上几个模块是在应用启动是完成了主机启动时的关键性作业;

我们再根据模块的依赖关系,可以看到主机在应用启动时,通过模块初始化任务的一个执行顺序如下所示:

```
RpcModule --> DotNettyTcpModule | TransactionModule | WebSocketModule | [RpcMonitorModule] 

--> GeneralHostModule(启动模块[StartUpModule])[DefaultGeneralHostModule|WebSocketHostModule|DefaultWebSocketHostModule] 

```
通过上述的依赖关系,我们可以知道:

1. Rpc模块在应用启动时是最早被执行;

2. 然后依次执行: DotNettyTcpModule | TransactionModule | WebSocketModule | [RpcMonitorModule] 等模块;

3. 最后执行应用启动模块指定的初始化方法;


在上述的过程中,Silky主机在启动时需要完成如下的工作:

1. 实现Rpc消息监听的订阅;

2. 解析应用服务与服务条目;

3. 启动Rpc消息监听;

4. 解析服务主机和注册该主机实例对应的端点;

5. 向服务注册中心更新或是获取服务元数据(应用服务描述信息和服务条目描述信息);

6. 向服务注册中心注册该服务当前实例的端点以及从服务注册中心获取该服务对应的所有实例；

7. 通过心跳的方式从服务注册中心获取最新的服务元数据以及服务实例信息;

在下面的篇章中,我们将着重介绍上述的过程是如何实现的。