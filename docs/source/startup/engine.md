---
title: 服务引擎
lang: zh-cn
---

## 构建服务引擎

在[注册Silky微服务应用](/source/startup/host.html#注册silky微服务应用)一节中,我们了解到在`ConfigureServices`阶段,通过`IServiceCollection`的扩展方法`AddSilkyServices<T>()`除了注册必要的服务之外,更主要的是构建了服务引擎(`IEngine`)。

下面,我们学习在`IServiceCollection`的扩展方法`AddSilkyServices<T>()`中完成了什么样的工作。如下所示的代码为在包 **Silky.Core** 的 [ServiceCollectionExtensions.cs](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/ServiceCollectionExtensions.cs)中提供的扩展方法`AddSilkyServices<T>()`。

```csharp
public static IEngine AddSilkyServices<T>(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment) where T : StartUpModule
{
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // 指定通信管道的加密传输协议
    CommonSilkyHelpers.DefaultFileProvider = new SilkyFileProvider(hostEnvironment); // 构建文件服务提供者
    services.TryAddSingleton(CommonSilkyHelpers.DefaultFileProvider);  // 向services注册单例的文件服务提供者
    var engine = EngineContext.Create(); // 创建单例的服务引擎
    services.AddOptions<AppSettingsOptions>()
        .Bind(configuration.GetSection(AppSettingsOptions.AppSettings)); // 新增AppSettingsOptions配置
    var moduleLoader = new ModuleLoader(); // 创建模块加载器
    engine.LoadModules<T>(services, moduleLoader); // 加载所有模块
    services.TryAddSingleton<IModuleLoader>(moduleLoader); // 注册单例的模块加载器
    services.AddHostedService<InitSilkyHostedService>();  // 注册 InitSilkyHostedService 后台任务服务,该服务用于初始化各个模块的任务或是在应用停止时释放模块资源
    services.AddSingleton<ICancellationTokenProvider>(NullCancellationTokenProvider.Instance); //注册默认的CancellationTokenProvider
    engine.ConfigureServices(services, configuration, hostEnvironment); // 通过服务引擎扫描所有IConfigureService接口的类,其实现类可以通过IServiceCollection对服务进行注册;以及通过各个模块的ConfigureServices方法对服务进行注册
    return engine; // 返回服务引擎对象
}
```

创建服务引擎的对象方法如下所示，我们可以看出,服务引擎在整个应用的生命周期是全局单例的。

```csharp
internal static IEngine Create()
{
    return Singleton<IEngine>.Instance ?? (Singleton<IEngine>.Instance = new SilkyEngine()); // 服务引擎在应用的整个生命周期是单例的
}
```


通过我们对上述代码注释可以看出，在`AddSilkyServices<T>()`方法中,在该方法中做了如下关键性的工作:

1. 构建了一个关键性的对象 **文件服务提供者(`SilkyFileProvider`)** ,该对象主要用于扫描或是获取指定的文件(例如应用程序集等)以及提供文件夹等帮助方法;

2. 使用`EngineContext`创建了服务引擎对象`SilkyEngine`对象;

3. 使用`IServiceCollection`注册了必要的核心的对象,如:`SilkyFileProvider`、`ModuleLoader`、`NullCancellationTokenProvider`等;

4. 创建模块加载器`ModuleLoader`对象,并通过服务引擎解析、加载silky模块,需要指出的是,在这里我们需要指定启动模块,系统会根据启动模块指定的依赖关系进行排序;

5. 注册后台任务服务`InitSilkyHostedService`,该服务用于初始化各个模块的任务或是在应用停止时释放模块资源;在各个模块的初始化工作中完成了很多核心的工作，例如:对应用服务以及服务条目的解析、服务元数据的注册、服务实例的注册与更新、Rpc消息监听者的启动等等;

6. 在调用服务引擎的`ConfigureServices()`方法时,通过服务引擎扫描所有`IConfigureService`接口的类,通过反射创建实现类的对象,通过`IServiceCollection`对服务进行注册;以及通过遍历所有的Silky模块实例,通过模块的提供的`ConfigureServices()`的方法通过`IServiceCollection`对服务进行注册。

::: tip 提示

如果熟悉 [nopCommerce](https://github.com/nopSolutions/nopCommerce) 框架的小伙伴们应该注意到,`SilkyEngine`服务引擎的作用与构建与该框架的设计基本是一致的。

:::

## 服务引擎的作用

服务引擎的`SilkyEngine`的作用主要由如下几点:

1. 通过模块加载器`ModuleLoader`解析和加载模块，关于模块如何解析和加载,请查看[下一节模块](#)内容;

2. 实现服务的依赖注入,本质上来说要么通过`IServiceCollection`服务实现服务注册,要么通过Autufac提供的`ContainerBuilder`实现服务注册;

服务引擎实现服务的依赖注入主要由如下几种方式实现:

2.1 通过扫描所有`IConfigureService`接口的类,并通过反射的方式构建实现类的对象,然后可以通过`IServiceCollection`对服务进行注册;以及通过遍历所有的Silky模块实例,通过模块的提供的`ConfigureServices()`的方法通过`IServiceCollection`对服务进行注册。

如下代码为服务引擎提供的`ConfigureServices()`方法源码:

```csharp
// SilkyEngine 实现的ConfigureServices注册服务的方法
public void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment hostEnvironment)
{
    _typeFinder = new SilkyAppTypeFinder(); // 创建类型查找器
    ServiceProvider = services.BuildServiceProvider();
    Configuration = configuration;
    HostEnvironment = hostEnvironment;
    HostName = Assembly.GetEntryAssembly()?.GetName().Name;  // 解析应用服务主机名称

    var configureServices = _typeFinder.FindClassesOfType<IConfigureService>(); // 通过查找器查找所有的`IConfigureService`实现类

    var instances = configureServices
        .Select(configureService => (IConfigureService)Activator.CreateInstance(configureService));  // 通过反射的方式创建`IConfigureService`实现类的实例

    foreach (var instance in instances) // 遍历`IConfigureService`的实现类的实例，并通过其实例实现通过IServiceCollection对服务的注册
        instance.ConfigureServices(services, configuration);
    // configure modules 
    foreach (var module in Modules) // 遍历各个模块,通过各个模块提供`ConfigureServices`实现服务的注册
        module.Instance.ConfigureServices(services, configuration);

    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

}
```

在上述代码中,我们可以看到在该方法体内主要完成如下工作:

A) 创建类型查找器、构建服务提供者以及为配置器、主机环境变量、主机名等赋值;

B) 使用类型查找器查找到所有`IConfigureService`实现类,并通过反射的方式创建其实例,遍历其实例,其实例通过`IServiceCollection`实现对服务的注册;

C) 遍历所有的模块,通过模块的实例提供的`ConfigureServices()`方法,通过`IServiceCollection`实现对服务的注册;

2.2 在上一章[注册silky微服务应用](/source/startup/host#注册silky微服务应用)中有指出, 执行`ContainerBuilder`方法时，主要通过`Autofac`的`ContainerBuilder`实现服务的依赖注册。

```csharp
public static IHostBuilder RegisterSilkyServices<T>(this IHostBuilder builder)
  where T : StartUpModule
{   
    // 其他代码略...
    builder
      .UseServiceProviderFactory(new AutofacServiceProviderFactory()) // 替换服务提供者工作类
      .ConfigureContainer<ContainerBuilder>(builder => // 通过ContainerBuilder实现服务依赖注册
        {
            engine.RegisterModules(services, builder);
            engine.RegisterDependencies(builder);
        })
}
```

我们看到,如何通过`ContainerBuilder`实现服务注册，也是通过服务引擎巧妙的实现：一种方式是通过模块，另外一种方式是通过约定的依赖方式。

2.2.1 通过模块注册服务

在`SilkyModule`的定义中,我们看到模块的基类是`Autofac.Module`,我们在遍历所有的模块实例的过程中，通过`ContainerBuilder`提供的`RegisterModule()`方法实现模块指定的服务的注册。换句话说,就是在在执行`RegisterModule()`的方法过程中,Autofac会调用模块的提供的`RegisterServices(ContainerBuilder builder)`实现具体的服务注册。

```csharp
public void RegisterModules(IServiceCollection services, ContainerBuilder containerBuilder)
{
    containerBuilder.RegisterInstance(this).As<IModuleContainer>().SingleInstance();
    var assemblyNames = ((AppDomainTypeFinder)_typeFinder).AssemblyNames;
    foreach (var module in Modules)
    {
        if (!assemblyNames.Contains(module.Assembly.FullName))
        {
            ((AppDomainTypeFinder)_typeFinder).AssemblyNames.Add(module.Assembly.FullName);
        }

        containerBuilder.RegisterModule((SilkyModule)module.Instance);
    }
}
```

所以在Silky模块的定义[SilkyModule](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/Modularity/SilkyModule.cs)中,提供了如下虚方法(`RegisterServices`),实际上是Autofac的基类`Autofac.Module`的一个基础方法,在调用`containerBuilder.RegisterModule((SilkyModule)module.Instance)`时，底层会通过调用模块的`Load()`实现模块的具体服务的注册。在`Load()`方法中,每个模块会调用`RegisterServices(builder)`实现通过`ContainerBuilder`对服务进行注册。


```csharp
protected override void Load([NotNull] ContainerBuilder builder)
{
    base.Load(builder);
    RegisterServices(builder);
}
```

所以,Silky具体的模块可以通过重写`RegisterServices([NotNull] ContainerBuilder builder)`实现该模块使用`ContainerBuilder`实现服务的依赖注册。

```csharp
protected virtual void RegisterServices([NotNull] ContainerBuilder builder)
{
}
```

::: tip 提示

使用`ContainerBuilder`实现服务的注册和通过`IServiceCollection`实现服务的注册的效果是一致的;使用`ContainerBuilder`实现服务的注册的优势在于支持命名服务的注册。也就是在服务注册的过程中,可以给服务起个名字,在服务解析的过程中，通过名称去解析到指定名称的接口的实现的对象。

:::

2.2.2 通过约定注册服务

服务引擎`SilkyEngine`通过调用`RegisterDependencies()`方法,使用`ContainerBuilder`实现对约定的规范的服务进行注册。

```csharp
 public void RegisterDependencies(ContainerBuilder containerBuilder)
{
    containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();
    containerBuilder.RegisterInstance(_typeFinder).As<ITypeFinder>().SingleInstance();

    var dependencyRegistrars = _typeFinder.FindClassesOfType<IDependencyRegistrar>();
    var instances = dependencyRegistrars
        .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
        .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);
    foreach (var dependencyRegistrar in instances)
        dependencyRegistrar.Register(containerBuilder, _typeFinder);
}
```

在上面的代码中,我们看到通过构建约定注册器(`IDependencyRegistrar`)的实例,通过约定注册器实现指定服务的注册。系统存在两个默认的约定注册器: 

(1) [DefaultDependencyRegistrar](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/DependencyInjection/DefaultDependencyRegistrar.cs),该服务注册器可以实现对标识接口的服务注册;
  
  A) 对继承`ISingletonDependency`的类注册为单例;
  B) 对继承`ITransientDependency`的类注册为瞬态;
  C) 对继承`IScopedDependency`的类注册为范围;

(2) [NamedServiceDependencyRegistrar](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Core/DependencyInjection/NamedServiceDependencyRegistrar.cs) 实现了对命名服务的注册;在某个类继承上述标识接口时,如果通过`InjectNamedAttribute`特性对服务进行命名,那么该服务的将会被命名为该名称的服务,在解析该服务的时候,可以通过名称进行解析。
例如:


```csharp
// 该服务将会被注册为范围的,并被命名为:DemoService,在服务解析过程中可以通过服务名 DemoService 解析到
[InjectNamed("DemoService")]
public class DemoService : IScopedDependency
{

}

```

3. 服务引擎提供了多种判断服务是否注册以及服务解析方法;

4. 服务引擎提供了获取指定的配置项的方法;

5. 可以通过服务引擎获取类型查找器(`TypeFinder`)、服务配置器(`Configuration`)、主机环境变量提供者(`IHostEnvironment`)、以及主机名(`HostName`)等信息。


## 获取和使用服务引擎

在开发过程中，可以通过`EngineContext.Current`获取服务引擎,并使用服务引擎提供的各个方法,例如:判断服务是否注册、解析服务、获取配置类、获取当前原因的主机名称、或是使用类型查找器(`TypeFinder`)、服务配置器(`Configuration`)、主机环境变量提供者(`IHostEnvironment`)等。

::: tip 提示

在开发过程中,使用服务引擎的大部分场景是，在不方便实现对某个服务进行构造注入的场景下,通过服务引擎实现对某个服务解析,从而得到该服务的实例。

:::