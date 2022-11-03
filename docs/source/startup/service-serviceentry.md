---
title: 服务与服务条目的解析
lang: zh-cn
---

## 概念

### 对外提供访问接口的服务

这里的 **服务** 与前文中所指的通过`ServiceCollection` 或是通过`ContainerBuilder`实现各个类的依赖关系注入所述的 **服务注册** 中的服务的概念是不一样的。

在这里所指的 **服务** 是指在一个Silky应用中,对应用外提供访问能力的接口，它与传统MVC框架中的控制器概念相对应。

在silky应用中,我们通过对接口添加`[ServiceRoute]`特性,就可以定义 **应用服务**,在该服务中定义的方法我们称之为 **服务条目**, **服务条目** 与之对应的概念是传统MVC中的 **Action**,在silky应用中,我们通过RPC调用的方式与其他微服务应用的服务条目进行远程通信;

**应用服务** 的主要属性如下:

| 属性名 | 名称  | 备注           |
|:-------|:------|:--------------|
| Id | 服务Id  | 由该服务的完全限定名生成 |
| ServiceDescriptor | 服务描述符  | 将会以服务元数据的方式注册到服务注册中心 |
| IsLocal | 是否是本地服务  | 在该应用内是否存在实现类,在运行时确定是由本地服务执行器执行还是通过RPC调用 |
| ServiceType | 应用服务对应的类型`Type`  |  |
| ServiceProtocol | 服务协议  | 该服务对应的服务协议 |
| ServiceEntries | 该服务对应的所有的服务条目  | 该服务定义的所有方法 |

**服务条目**的主要属性如下:

| 属性名 | 名称  | 备注           |
|:-------|:------|:--------------|
| Id | 服务条目Id  | 该服务条目对应方法的完全限定名 + 参数名 + 对应的Http请求方法名 |
| ServiceId | 服务Id  | 对应的服务Id |
| ServiceType | 应用服务对应的类型`Type`  |
| IsLocal | 是否是本地服务条目  | |  |
| MethodExecutor | 方法执行者  | 该服务条目对应方法的ObjectMethodExecutor |
| FallbackProvider | 失败重试提供器  | 可为空,当服务条目调用失败后,通过它可以获取到配置的失败回调的方法 |
| SupportedRequestMediaTypes | 支持的http请求的媒体类型  |  |
| SupportedResponseMediaTypes |  支持的http响应的媒体类型 |  |
| Router |  对应的Http路由 | 主要包括路由模板、对应的Http方法、Http路径(WebAPI)、以及解析http Path参数  |
| MethodInfo |  对应的方法 |   |
| ReturnType |  服务条目对应方法返回值的类型 |   |
| ParameterDescriptors |  参数描述符 | 用于描述和解释该方法对应的参数说明  |
| CustomAttributes |  该服务条目所有的特性 |   |
| ClientFilters |  客户端过滤器 |  非本地服务条目在执行RPC远程调用时根据客户端过滤器的排序依次执行过滤器方法 |
| ClientFilters |  服务端过滤器 |  本地服务条目在执行实现的业务方法时根据服务端过滤器排序依次执行过滤器方法 |
| AuthorizeData |  身份认证数据 |   |
| GovernanceOptions |  服务条目在执行过程中的实现服务治理的参数配置 |  例如: 超时时间、负载均衡策略、是否允许服务熔断、发生非业务类异常N次后出现熔断、熔断时长、重试次数、是否禁用外网等等  |
| ServiceEntryDescriptor |  服务条目描述符 | 该服务条目对应的服务条目描述符,将会以元数据的方式注册到服务注册中心  |






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
public class DefaultServiceGenerator : IServiceGenerator
{
    private readonly IIdGenerator _idGenerator;
    private readonly ITypeFinder _typeFinder;
    private readonly IServiceEntryManager _serviceEntryManager;
    public DefaultServiceGenerator(IIdGenerator idGenerator,
        ITypeFinder typeFinder,
        IServiceEntryManager serviceEntryManager)
    {
        _idGenerator = idGenerator;
        _typeFinder = typeFinder;
        _serviceEntryManager = serviceEntryManager;
    }

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
}

// 其他代码略...

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

在上面的源码解读中,我们看到,在应用服务解析过程中,应用服务所持有的服务条目是根据服务条目管理器根据`serviceId`提供的`_serviceEntryManager.GetServiceEntries(serviceId)`获取。

服务条目如何解析和获取是由默认的服务条目管理器`DefaultServiceEntryManager`负责,服务条目管理器跟服务管理器一样,都是被注册为 **单例的**, 在整个应用生命周期,只会被创建一次。并且在构造器中实现服务条目的创建并对服务条目进行缓存。

```csharp
public class DefaultServiceEntryManager : IServiceEntryManager
{
    private IEnumerable<ServiceEntry> m_localServiceEntries;  //用于缓存本地服务条目
    private IEnumerable<ServiceEntry> m_allServiceEntries;  // 用于缓存全部服务条目
    private IChangeToken? _changeToken;

    public DefaultServiceEntryManager(IEnumerable<IServiceEntryProvider> providers)
    {
        UpdateEntries(providers);
    }

    private void UpdateEntries(IEnumerable<IServiceEntryProvider> providers)
    {
        var allServiceEntries = new List<ServiceEntry>();
        foreach (var provider in providers) // 遍历所有服务条目提供者,有服务条目服务者创建服务条目
        {
            var entries = provider.GetEntries();
            foreach (var entry in entries)
            {
                if (allServiceEntries.Any(p => p.ServiceEntryDescriptor.Id == entry.ServiceEntryDescriptor.Id))
                {
                    throw new InvalidOperationException(
                        $"Locally contains multiple service entries with Id: {entry.ServiceEntryDescriptor.Id}");
                }
                allServiceEntries.Add(entry);
            }
        }
        if (allServiceEntries.GroupBy(p => p.Router).Any(p => p.Count() > 1))
        {
            throw new SilkyException(
                "There is duplicate routing information, please check the service routing you set");
        }
        m_allServiceEntries = allServiceEntries;
        m_localServiceEntries = allServiceEntries.Where(p => p.IsLocal);
    }

    public void Update(ServiceEntry serviceEntry)
    {
        m_allServiceEntries = m_allServiceEntries
            .Where(p => !p.ServiceEntryDescriptor.Id.Equals(serviceEntry.ServiceEntryDescriptor.Id))
            .Append(serviceEntry);
        if (serviceEntry.IsLocal)
        {
            m_localServiceEntries = m_localServiceEntries
                .Where(p => !p.ServiceEntryDescriptor.Id.Equals(serviceEntry.ServiceEntryDescriptor.Id))
                .Append(serviceEntry);
        }
        OnUpdate?.Invoke(this, serviceEntry);
    }

   // 其他代码略...
}
```

从上面的源码我们可以看到,在服务条目管理器`DefaultServiceEntryManager`被创建时,会调用`UpdateEntries(providers)`通过遍历所有的服务条目提供者生成服务条目，并对服务条目进行缓存，服务条目不允许重复。

### 服务条目提供者

与服务提供者一样,开发者也可以根据自己的约定实现自己的服务条目提供者,Silky框架实现了默认的服务提供者`DefaultServiceEntryProvider`。

```csharp
public class DefaultServiceEntryProvider : IServiceEntryProvider
{
    public IReadOnlyList<ServiceEntry> GetEntries()
    {
        var serviceTypeInfos = ServiceHelper.FindAllServiceTypes(_typeFinder);
        if (!EngineContext.Current.IsContainHttpCoreModule())
        {
            serviceTypeInfos = serviceTypeInfos.Where(p =>
                p.Item1.GetCustomAttributes().OfType<DashboardAppServiceAttribute>().FirstOrDefault() == null);
        }
        var entries = new List<ServiceEntry>();
        foreach (var serviceTypeInfo in serviceTypeInfos)
        {
            Logger.LogDebug("The Service were be found,type:{0},IsLocal:{1}", serviceTypeInfo.Item1.FullName,
                serviceTypeInfo.Item2);
            entries.AddRange(_serviceEntryGenerator.CreateServiceEntry(serviceTypeInfo));
        }

        return entries;
    }
   
// 其他代码略...

}
```

服务条目提供者创建服务条目的过程如下:

1. 查找到所有的服务类型`IEnumerable<(Type, bool)> serviceTypeInfos`，其中：元组的第一个参数表示服务类型，第二个参数表示是否是本地服务;
2. 是否包含`HttpCoreModule`模块,如果不包含,那么忽略标识了`[DashboardAppService]`的服务条目;
3. 遍历所有的服务类型，通过服务条目生成器`IServiceEntryGenerator`创建该服务定义的所有服务条目;


### 服务条目生成器

服务条目生成器`DefaultServiceEntryGenerator`通过遍历服务类型定义的所有方法,以及该方法标识的`HttpMethod`,并且依次创建服务条目;

```csharp
public class DefaultServiceEntryGenerator : IServiceEntryGenerator
{
   public IEnumerable<ServiceEntry> CreateServiceEntry((Type, bool) serviceType)
   {
        var serviceBundleProvider = ServiceDiscoveryHelper.GetServiceBundleProvider(serviceType.Item1);
        var methods = serviceType.Item1.GetTypeInfo().GetMethods();
        foreach (var method in methods)
        {
            var httpMethodInfos = method.GetHttpMethodInfos();
            foreach (var httpMethodInfo in httpMethodInfos)
            {
                yield return Create(method,
                    serviceType.Item1,
                    serviceType.Item2,
                    serviceBundleProvider,
                    httpMethodInfo);
            }
        }
    }

    private ServiceEntry Create(MethodInfo method,
            Type serviceType,
            bool isLocal,
            IRouteTemplateProvider routeTemplateProvider,
            HttpMethodInfo httpMethodInfo)
        {
            var serviceName = serviceType.Name;
            var serviceEntryId = _idGenerator.GenerateServiceEntryId(method, httpMethodInfo.HttpMethod);
            var serviceId = _idGenerator.GenerateServiceId(serviceType);
            var parameterDescriptors = _parameterProvider.GetParameterDescriptors(method, httpMethodInfo);
            if (parameterDescriptors.Count(p => p.IsHashKey) > 1)
            {
                throw new SilkyException(
                    $"It is not allowed to specify multiple HashKey,Method is {serviceType.FullName}.{method.Name}");
            }
            
            var serviceEntryTemplate =
                TemplateHelper.GenerateServerEntryTemplate(routeTemplateProvider.Template, parameterDescriptors,
                    httpMethodInfo, _governanceOptions.ApiIsRESTfulStyle,
                    method.Name);

            var router = new Router(serviceEntryTemplate, serviceName, method, httpMethodInfo.HttpMethod);
            Debug.Assert(method.DeclaringType != null);
            var serviceEntryDescriptor = new ServiceEntryDescriptor()
            {
                Id = serviceEntryId,
                ServiceId = serviceId,
                ServiceName = routeTemplateProvider.GetServiceName(serviceType),
                ServiceProtocol = ServiceHelper.GetServiceProtocol(serviceType, isLocal, false),
                Method = method.Name,
            };

            var metaDataList = method.GetCustomAttributes<MetadataAttribute>();

            foreach (var metaData in metaDataList)
            {
                serviceEntryDescriptor.Metadatas.Add(metaData.Key, metaData.Value);
            }

            var serviceEntry = new ServiceEntry(router,
                serviceEntryDescriptor,
                serviceType,
                method,
                parameterDescriptors,
                isLocal,
                _governanceOptions);

            if (serviceEntry.NeedHttpProtocolSupport())
            {
                serviceEntryDescriptor.Metadatas.Add(ServiceEntryConstant.NeedHttpProtocolSupport, true);
            }
            
            return serviceEntry;
        }

   // 其他代码略...

   
}
```

如果一个方法被标识了多个`HttpMethod`,那么将会生成两个不同的服务条目,如果没有被标识`HttpMethod`特性,那么将会根据命名的规则默认返回对应的`HttpMethod`方法:

```csharp
        public static ICollection<HttpMethodInfo> GetHttpMethodInfos(this MethodInfo method)
        {
            var httpMethodAttributeInfo = method.GetHttpMethodAttributeInfos();
            var httpMethods = new List<HttpMethodInfo>();

            foreach (var httpMethodAttribute in httpMethodAttributeInfo.Item1)
            {
                var httpMethod = httpMethodAttribute.HttpMethods.First().To<HttpMethod>();
                if (!httpMethodAttributeInfo.Item2)
                {
                    if (method.Name.StartsWith("Create"))
                    {
                        httpMethod = HttpMethod.Post;
                    }

                    if (method.Name.StartsWith("Update"))
                    {
                        httpMethod = HttpMethod.Put;
                    }

                    if (method.Name.StartsWith("Delete"))
                    {
                        httpMethod = HttpMethod.Delete;
                    }

                    if (method.Name.StartsWith("Search"))
                    {
                        httpMethod = HttpMethod.Get;
                    }

                    if (method.Name.StartsWith("Query"))
                    {
                        httpMethod = HttpMethod.Get;
                    }

                    if (method.Name.StartsWith("Get"))
                    {
                        httpMethod = HttpMethod.Get;
                    }
                }

                httpMethods.Add(new HttpMethodInfo()
                {
                    IsSpecify = httpMethodAttributeInfo.Item2,
                    Template = httpMethodAttribute.Template,
                    HttpMethod = httpMethod
                });
            }

            return httpMethods;
        }
```

创建服务条目的过程如下所述:

1. 通过Id生成器`IIdGenerator`依次生成服务Id和服务条目Id;
2. 通过参数提供者获取该方法对应的参数描述符` _parameterProvider.GetParameterDescriptors(method, httpMethodInfo)`;
3. 通过`TemplateHelper.GenerateServerEntryTemplate()`为该方法生成路由模板，并创建该方法对应的路由器`router`;
4. 创建服务条目描述符,并根据方法的特性`[Metadata]`更新服务描述符的元数据;
5. 调用服务条目的构造方法创建服务条目;
6. 更新服务条目描述符的元数据;

服务条目治理构造方法如下所示,在服务条目构造器中完成了如下一系列的任务:

```csharp
    internal ServiceEntry(IRouter router,
            ServiceEntryDescriptor serviceEntryDescriptor,
            Type serviceType,
            MethodInfo methodInfo,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors,
            bool isLocal,
            GovernanceOptions governanceOptions)
        {
            Router = router;
            _serviceEntryDescriptor = serviceEntryDescriptor;
            ParameterDescriptors = parameterDescriptors;
            _serviceType = serviceType;
            IsLocal = isLocal;
            MethodInfo = methodInfo;
            CustomAttributes = MethodInfo.GetCustomAttributes(true);
            (IsAsyncMethod, ReturnType) = MethodInfo.ReturnTypeInfo();
            GovernanceOptions = new ServiceEntryGovernance(governanceOptions); // 根据服务治理配置创建服务条目治理属性

            var governanceProvider = CustomAttributes.OfType<IGovernanceProvider>().FirstOrDefault();
            ReConfiguration(governanceProvider); // 更新服务条目的服务治理配置属性

            _methodExecutor = methodInfo.CreateExecutor(serviceType); // 创建方法执行器
            Executor = CreateExecutor();  //创建服务条目执行器
            AuthorizeData = CreateAuthorizeData(); // 解析服务条目的身份认证元数据

            ClientFilters = CreateClientFilters();  // 解析客户端过滤器
            ServerFilters = CreateServerFilters();  // 解析服务端过滤器
            CreateFallBackExecutor();   // 创建失败回调执行器
            CreateDefaultSupportedRequestMediaTypes();  // 创建默认的请求媒体类型 
            CreateDefaultSupportedResponseMediaTypes(); // 创建默认的响应媒体类型
            CreateCachingInterceptorDescriptors();  // 创建缓存拦截描述符
        }
```

## 服务与服务条目的解析过程

通过上文所述,我们知道服务与服务条目是在其 **管理器** 创建的时候进行构造解析的, **管理器**是单例的,也就是说在整个应用的生命周期中,服务与服务条目都只会被创建一次，并存在于应用的内存中。那么服务与服务条目是在服务条目的什么时候进行解析的呢?

1. 我们看到在服务生成器`DefaultServiceGenerator`中看到,通过构造注入服务条目管理器接口`IServiceEntryManager`,也就是说,在生成服务之前必须要先创建服务条目管理器的实例，在解析服务之前需要先解析服务条目,也正是因为如此,所以可以在在解析服务的时候通过服务条目管理器`IServiceEntryManager`获取该服务对应的服务条目;

```csharp
public class DefaultServiceGenerator : IServiceGenerator
{
    private readonly IIdGenerator _idGenerator;
    private readonly ITypeFinder _typeFinder;
    private readonly IServiceEntryManager _serviceEntryManager;
    
    public DefaultServiceGenerator(IIdGenerator idGenerator,
        ITypeFinder typeFinder,
        IServiceEntryManager serviceEntryManager)
    {
        _idGenerator = idGenerator;
        _typeFinder = typeFinder;
        _serviceEntryManager = serviceEntryManager;
    }
}
```

2. 在Silky服务主机提供者`DefaultServerProvider` 中,我们看到通过构造注入应用服务管理器`IServiceManager`,也就是说在第一次获取Silky服务主机提供者的时候,需要创建应用服务管理器的实例,实现应用服务的解析；

```csharp
public class DefaultServerProvider : IServerProvider
{
    public ILogger<DefaultServerProvider> Logger { get; set; }
    private readonly IServer _server;
    private readonly IServiceManager _serviceManager;
    private readonly ISerializer _serializer;

    public DefaultServerProvider(IServiceManager serviceManager,
        ISerializer serializer)
    {
        _serviceManager = serviceManager;
        _serializer = serializer;
        Logger = EngineContext.Current.Resolve<ILogger<DefaultServerProvider>>();
        _server = new Server(EngineContext.Current.HostName);
    }
}
```

通过上述的描述,我们可以了解到,在应用启动过程中,在首次解析Silky服务主机提供者实例`DefaultServerProvider`的时候,Silky框架会首先进行服务条目的解析,然后再解析应用服务;由于其相应的服务管理器都是 **单例的**，在整个应用的生命周期中,服务与服务条目都只会被解析一次;

服务和服务条目被解析成功后，也会存在相应的**描述符**,随着应用的启动,描述符将会作为silky服务主机的一部分,将会随着应用服务主机的描述符注册到服务注册中心，服务注册中心将会更新整个微服务集群的注册信息(包括新增微服务主机信息、支持的服务与服务条目、以及主机实例的的终结点等等元数据信息)，集群的其他微服务主机实例将会通过心跳或是订阅的方式获取到整个集群最新的元数据,并通过更新到内存中;

接下来,我们将继续介绍在应用启动时,如何构建Silky服务主机(提供者)[Server](https://github.com/liuhll/silky/blob/main/framework/src/Silky.Rpc/Runtime/Server/Server.cs),并将其信息注册到服务注册中心;
