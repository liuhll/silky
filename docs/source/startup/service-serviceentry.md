---
title: 应用服务与服务条目的解析
lang: zh-cn
---

## 概念

### 对外提供访问接口的服务

这里的 **服务** 与前文中所指的通过`ServiceCollection` 或是通过`ContainerBuilder`实现各个类的依赖关系注入所述的 **服务注册** 中的服务的概念是不一样的。

在这里所指的 **服务** 是指在一个Silky应用中,对应用外提供访问能力的接口，它与传统MVC框架中的控制器概念相对应。

在silky应用中,我们通过对接口添加`[ServiceRoute]`特性,就可以定义 **应用服务**,在该服务中定义的方法我们称之为 **服务条目**, **服务条目** 与之对应的概念是传统MVC中的 **Action**,在silky应用中,我们通过RPC调用的方式与其他微服务应用的服务条目进行远程通信;

在本节中,我们主要叙述在应用启动时,Silky是如何对应用内定义的服务以及服务条目进行解析。

## 应用服务的解析

### 应用服务管理器

1. Silky框架中由服务管理器`ServiceManager`负责应用服务的解析和获取,服务管理器被注册为 **单例** 的,也就是说,应用服务管理器在整个应用的生命周期中只会被创建一次,在服务管理器`DefaultServiceManager`的构造器中,将会调用服务提供者的所有实现类，解析应用服务;

```csharp
public class DefaultServiceManager : IServiceManager
{
   private IEnumerable<Service> m_localServices; // 用于缓存本地应用服务
   private IEnumerable<Service> m_allServices; // 用于缓存所有应用服务

   public DefaultServiceManager(IEnumerable<IServiceProvider> providers) // 在服务管理器的构造器注入所有的服务提供者
   {
       UpdateServices(providers); // 使用注入所有的服务提供者实现解析应用服务
   }

   private void UpdateServices(IEnumerable<IServiceProvider> providers)
   {
       var allServices = new List<Service>();
       foreach (var provider in providers)
       {
           var services = provider.GetServices();
           allServices.AddRange(services);
       }
       if (allServices.GroupBy(p => p.Id).Any(p => p.Count() > 1))
       {
           throw new SilkyException(
               "There is duplicate service information, please check the service you set");
       }
       m_allServices = allServices.ToList();
       m_localServices = allServices.Where(p => p.IsLocal).ToList();
   }

// 其他代码略...

}


```

从上面的代码的代码我们可以看出,通过遍历所有的服务提供者,通过其方法`provider.GetServices()`解析该服务提供者规定的应用服务;silky框架提供了默认的服务提供者`DefaultServiceProvider`对标识了`[ServiceRoute]`的接口进行解析；

当然,开发者也可以根据需要对应用服务提供者进行扩展(通过实现`IServiceProvider`接口),实现对开发者对自己定义的应用服务进行解析;

通过服务Id进行分组判断应用服务是否重复，在一个用于中,应用服务时不允许重复的，如果定义了相同的应用服务接口,那么应用将会在启动时抛出异常;

服务管理器中定义了两个全局变量：一个用于缓存本地应用服务,一个用于缓存所有的应用服务; 

### 默认的服务提供者

2. Silky框架中,通过默认的服务提供者`DefaultServiceProvider`扫描标识了`[ServiceRoute]`的接口,然后通过遍历所有的服务类型,通过默认的服务生成器`DefaultServiceGenerator`实现创建应用服务对象;

```csharp
public IReadOnlyCollection<Service> GetServices()
{
    var serviceTypes = ServiceHelper.FindAllServiceTypes(_typeFinder);
    if (!EngineContext.Current.IsContainHttpCoreModule()) // 如果不包含HttpCoreModule模块,则忽略标识了`[DashboardAppService]`的应用服务
    {
        serviceTypes = serviceTypes.Where(p =>
            p.Item1.GetCustomAttributes().OfType<DashboardAppServiceAttribute>().FirstOrDefault() == null);
    }
    var services = new List<Service>();
    foreach (var serviceTypeInfo in serviceTypes)
    {
        services.Add(_serviceGenerator.CreateService(serviceTypeInfo));
    }
    if (EngineContext.Current.IsContainWebSocketModule())
    {
        var wsServiceTypes = ServiceHelper.FindServiceLocalWsTypes(_typeFinder);
        foreach (var wsServiceType in wsServiceTypes)
        {
            services.Add(_serviceGenerator.CreateWsService(wsServiceType));
        }
    }
    return services;
}
```
通过上述的代码我们看到：

2.1 通过服务帮助者类提供的`ServiceHelper.FindAllServiceTypes(_typeFinder)`查找到所有的应用服务的类型`serviceTypes`,如果不包含HttpCoreModule模块,则忽略标识了`[DashboardAppService]`的应用服务,然后遍历所有的服务类型,通过服务生成器`_serviceGenerator.CreateService(serviceTypeInfo)`生成应用服务对象;

查找应用服务类型的方法`ServiceHelper.FindAllServiceTypes(_typeFinder)`如下所示:

```csharp
public static IEnumerable<(Type, bool)> FindAllServiceTypes(ITypeFinder typeFinder)
{
    var serviceTypes = new List<(Type, bool)>();
    var exportedTypes = typeFinder.GetaAllExportedTypes();
    var serviceInterfaces = exportedTypes
            .Where(p => p.IsInterface
                        && p.GetCustomAttributes().Any(a => a is ServiceRouteAttribute)
                        && !p.IsGenericType
            )
        ;
    foreach (var entryInterface in serviceInterfaces)
    {
        serviceTypes.Add(
            exportedTypes.Any(t => entryInterface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                ? (entryInterface, true)
                : (entryInterface, false));
    }
    return serviceTypes;
}
```

从上面的代码我们可以看到框架是如何扫描应用服务的：首先从系统统扫描所有被标识了`[ServiceRoute]`特性的接口,然后遍历所有的接口,分析出该接口是否存在实现类,从而得到应用服务的类型信息`(TypeInfo,IsLocal)`;


2.2 如果当前应用包含`WebSocketModule`,则通过服务帮助类提供的`ServiceHelper.FindServiceLocalWsTypes(_typeFinder)`查找所有的支持websocket的`wsServiceTypes`,然后遍历所有的`wsServiceTypes`，然后通过`_serviceGenerator.CreateWsService(wsServiceType)`生成ws服务对象；

查找支持websocket的应用服务的方法`ServiceHelper.FindServiceLocalWsTypes(_typeFinder)`如下所示:

```csharp
public static IEnumerable<Type> FindServiceLocalWsTypes(ITypeFinder typeFinder)
{
    var types = typeFinder.GetaAllExportedTypes()
            .Where(p => p.IsClass
                        && !p.IsAbstract
                        && !p.IsGenericType
                        && p.GetInterfaces().Any(i =>
                            i.GetCustomAttributes().Any(a => a is ServiceRouteAttribute))
                        && p.BaseType?.FullName == ServiceConstant.WebSocketBaseTypeName
            )
            .OrderBy(p =>
                p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Select(q => q.Weight).FirstOrDefault()
            )
        ;
    return types;
}
```
从上面的方法我们可以看出,一个支持websocket应用服务必须为一个类, 其接口需要通过`[ServiceRoute]`特性标识,并且必须派生自`Silky.WebSocket.WsAppServiceBase`；

### 服务生成器(创建者)

3. 服务生成器`DefaultServiceGenerator`会通过服务类型生成两种不同类型的服务:

3.1 普通的应用服务(可以通过RPC协议与服务内部实现通信或是通过Http协议简介与外部实现通信),普通的应用服务通过如下代码生成：

```csharp

public Service CreateService((Type, bool) serviceTypeInfo)
{
    var serviceId = _idGenerator.GenerateServiceId(serviceTypeInfo.Item1);
    var serviceInfo = new Service()
    {
        Id = serviceId,
        ServiceType = serviceTypeInfo.Item1,
        IsLocal = serviceTypeInfo.Item2,
        ServiceProtocol = ServiceHelper.GetServiceProtocol(serviceTypeInfo.Item1, serviceTypeInfo.Item2, true),
        ServiceEntries =  _serviceEntryManager.GetServiceEntries(serviceId)
    };
    serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
    return serviceInfo;
}

```

我们看到，应用服务的输入参数是一个元组`(Type, bool) serviceTypeInfo`,元组的第一个参数表示服务的类型,第二个参数表示是否是本地服务;在应用内实现了应用服务接口则表示是一个本地应用服务，如果没有实现应用服务接口,则标识该服务时一个远程应用服务,在使用该服务提供的方法时，可以通过该服务生成的代理与具体的服务提供者进行RPC通信;

我们看到服务Id是通过Id生成器(`IIdGenerator`)生成的,服务Id的生成规则是该服务类型的完全限定名:

```csharp
public string GenerateServiceId(Type serviceType)
{
    Check.NotNull(serviceType, nameof(serviceType));
    return serviceType.FullName;
}
```

应用服务所持有的服务条目会根据服务条目管理器`IServiceEntryManager`通过服务Id获取到,服务条目如何生成;服务条目如何生成,我们将会在下一节进行叙述;

其中，比较重要的一点就是如果通过应用服务生成服务条目描述符`ServiceDescriptor`,服务描述符是一个POJO对象,可以被注册到服务注册中心,我们通过`CreateServiceDescriptor(serviceInfo)`生成该服务对应的服务描述符;

```csharp
        private ServiceDescriptor CreateServiceDescriptor(Service service)
        {
           
            var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(service.ServiceType);
            var serviceDescriptor = new ServiceDescriptor
            {
                ServiceProtocol = service.ServiceProtocol,
                Id = service.Id,
                ServiceName = serviceBundleProvider.GetServiceName(service.ServiceType),
                ServiceEntries = service.ServiceEntries.Select(p => p.ServiceEntryDescriptor).ToArray()
            };

            if (service.IsLocal)
            {
                var implementTypes = ServiceHelper.FindLocalServiceImplementTypes(_typeFinder, service.ServiceType);
                var serviceKeys = new Dictionary<string, int>();
                foreach (var implementType in implementTypes)
                {
                    var serviceKeyProvider = implementType.GetCustomAttributes().OfType<IServiceKeyProvider>()
                        .FirstOrDefault();
                    if (serviceKeyProvider != null)
                    {
                        if (serviceKeys.ContainsKey(serviceKeyProvider.Name))
                        {
                            throw new SilkyException(
                                $"The {service.ServiceType.FullName} set ServiceKey is not allowed to be repeated");
                        }

                        serviceKeys.Add(serviceKeyProvider.Name, serviceKeyProvider.Weight);
                    }
                }

                if (serviceKeys.Any())
                {
                    serviceDescriptor.Metadatas.Add(ServiceConstant.ServiceKey, serviceKeys);
                }
            }

            var metaDataList = service.ServiceType.GetCustomAttributes<MetadataAttribute>();
            foreach (var metaData in metaDataList)
            {
                serviceDescriptor.Metadatas.Add(metaData.Key, metaData.Value);
            }

            if (service.ServiceProtocol == ServiceProtocol.Ws)
            {
                serviceDescriptor.Metadatas.Add(ServiceConstant.WsPath,
                    WebSocketResolverHelper.ParseWsPath(service.ServiceType));
            }

            return serviceDescriptor;
        }
```

我们看到如果该服务是本地服务，则会查找该应用服务的对应的服务类,我们知道,一个应用服务接口是可以存在多个实现类的;其实现类可以通过特性`[ServiceKey]`标识,该特性有两个参数,一个是实现类的名称,一个是该实现类的权重，如下所示:

```csharp

[ServiceKey("v1", 3)]
public class TestAppService : ITestAppService
{
    
}

```

```csharp
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ServiceKeyAttribute : Attribute, IServiceKeyProvider
    {
        public ServiceKeyAttribute(string name, int weight)
        {
            Name = name;
            Weight = weight;
        }

        public string Name { get; }

        public int Weight { get; }
    }
```

前端在发送http请求的时候,可以通过在请求头携带`ServiceKey`来指定调用的实现类,如果没有指定`ServiceKey`的话,则执行权重大的实现类;

除此之外,应用服务还可以通过特性`[Metadata(key,value)]`来标识应用服务,通过其来追加该服务的元数据;

3.2 支持websocket的服务，在上一节中我们介绍了怎么查找支持websocket的服务；支持websocket的服务要求该主机必须包含`WebSocketModule`,查找该实现类后，通过服务生成器的`CreateWsService(wsServiceType)`创建支持websocket的服务:

```csharp
public Service CreateWsService(Type wsServiceType)
{
    var wsPath = WebSocketResolverHelper.ParseWsPath(wsServiceType);
    var serviceId = WebSocketResolverHelper.Generator(wsPath);
      
    var serviceInfo = new Service()
    {
        Id = serviceId,
        ServiceType = wsServiceType,
        IsLocal = true,
        ServiceProtocol = ServiceProtocol.Ws,
        ServiceEntries =  _serviceEntryManager.GetServiceEntries(serviceId)
    };
    serviceInfo.ServiceDescriptor = CreateServiceDescriptor(serviceInfo);
    return serviceInfo;
}
```

websocket服务生成服务Id的方法不一样,是通过该服务对应的websocket对应的webAPI的路径生成的,其他的属性赋值与普通应用服务的方式一致。

## 服务条目的解析