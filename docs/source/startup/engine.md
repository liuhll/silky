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
    engine.ConfigureServices(services, configuration, hostEnvironment); // 通过服务引擎扫描所有IConfigureService接口的类,其实现类可以通过IServiceCollection对服务进行注册
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

6. 通过服务引擎扫描所有`IConfigureService`接口的类,其实现类可以通过`IServiceCollection`对服务进行注册;

## 服务引擎的作用


