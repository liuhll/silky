---
title: 应用服务和服务条目
lang: zh-cn
---

## 服务的定义

服务接口是微服务定义服务的基本单位，定义的应用服务接口可以被其他微服务引用，其他微服务通过rpc框架与该微服务进行通信。

通过`ServiceRouteAttribute`特性对一个接口进行标识即可成为一个应用服务接口。

例如:

```csharp
[ServiceRoute]
public interface ITestAppService
{
}

```

虽然我们通过使用`[ServiceRoute]`特性可以对任何一个接口标识为一个服务，服务定义的方法会通过应用服务的模板和方法特性的模板生成对应的webapi(该方法没有服务特性标识为禁用外网)。但是良好的命名规范可以为我们构建服务省去很多不必要的麻烦(通俗的说就是:**约定大约配置**)。

一般地,我们推荐使用`AppService`作为定义的服务的后缀。即推荐使用`IXxxxAppService`作为应用服务接口名称,默认生成的服务模板为:`api/{appservice}`,使用`XxxxAppService`作为应用服务实现类的名称。

路由特性(`ServiceRouteAttribute`)可以通过`template`对服务路由模板进行设置。路由模板可以通过`{appservice=templateName}`设置服务的名称。

| 属性名称 | 说明   |  缺省值  | 
|:---------|:----- |:--------| 
| template | 在对服务接口标识为服务路由时,可以通过`[ServiceRoute("{appservice=templateName}")]`指定应用服务接口的路由模板。`templateName`的缺省值名称为对应服务的名称 | api/{appservice} | 



## 服务条目

**服务条目(ServiceEntry)**: 服务接口中定义的每一个方法都会生成微服务集群的一个服务条目。对微服务应用本身而言,服务条目就相当于MVC中的`Action`，应用服务就相当于`Controller`。

### 根据服务条目生成WebAPI

应用接口被web主机应用或是网关引用后,会根据服务应用接口的路由模板和服务条目方法的Http动词特性指定的路由信息或是方法名称生成相应的webapi,服务条目生成的WebAPI支持restfulAPI风格。


服务条目生成webapi的规则为:

1. 禁止集群外部访问的服务条目(`[Governance(ProhibitExtranet = true)]`)不会生成webapi;

2. 可以通过` [ServiceRoute("{appservice=templateName}")]`为应用接口指定统一的路由模板;

3. 如果服务条目方法没有被http谓词特性标识,那么生成的webapi的http请求动词会根据服务条目的方法名称生成,如果没有匹配到相应的服务条目方法,则会根据服务条目的方法参数；

4. 服务条目方法可以通过http谓词特性进行标识,并且http谓词特性还支持对服务条目指定路由模板,路由模板的指定还支持对路由参数进行约束;

```

服务条目生成的webAPI = 应用接口条目路由模板 + “方法名称||Http特性指定的路由特性”

```

如果不存在Http谓词特性标识情况下,生成的webapi路由说明(例如,应用接口名称为:`ITestAppService`,路由模板未被改写):

| 方法名称 | 生成的webAPI路径 | Http请求动词 |
|:--------|:---------------|:------------|
| GetXXX | /api/test | get |
| SearchXXX | /api/test/search | get|
| CreateXXX| /api/test | post |
| UpdateXXX | /api/test | put | 
| DeleteXXX | /api/test | delete | 

存在Http谓词情况下,生成的webapi的请求动词会根据服务条目标识的http谓词特性来决定，开发者还可以通过http谓词特性为服务条目的的路由进行调整,并且支持路由参数的形式,例如:

| 方法名称 | 生成的webAPI路径 | http谓词特性 | Http请求动词 |
|:--------|:---------------|:------------|:------------|
| GetXXX | /api/test/{id} | [HttpGet("{id:strig}")] | get |
| DeleteXXX | /api/test/name/{name} | [HttpDelete("name/{name:strig}")] | delete |
| UpdateXXX | /api/test/email | [HttpPatch("email")] | patch |
| CreateXXX | /api/test/user | [HttpPost("user")] | post |


### 服务条目的治理特性

开发者可以通过配置文件对服务条目的治理进行统一配置,除此之外可以通过`Governance`特性为服务条目方法进行标识,通过其属性对服务条目进行治理。通过`Governance`特性对服务条目方法注解后,服务条目的治理属性将会被该特性重写。

服务条目治理的属性请参考[服务条目治理属性配置](#)。

### 缓存拦截

在服务应用接口被其他微服务应用引用后,可以通过缓存拦截特性(`GetCachingIntercept`)在rpc通信过程中,直接从分布式缓存中读取数据,避免通过网络通信,而从提高系统性能。

更多关于缓存拦截请参考[缓存拦截](caching)。

### 服务条目的例子

```csharp
    [ServiceRoute]
    public interface ITestAppService
    {
        ///  新增接口测试([post]/api/test)
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Create(TestInput input);

        ///  更新接口测试([put]/api/test)
        Task<string> Update(TestInput input);

        /// 删除接口测试([delete]/api/test)
        [RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "name:{0}")]
        [Transaction]
        Task<string> Delete([CacheKey(0)] string name);

        /// 查询接口测试([get]/api/test/search)
        [HttpGet]
        Task<string> Search([FromQuery] TestInput query);

        /// 以表单格式提交数据([post]/api/test/form)
        [HttpPost]
        string Form([FromForm] TestInput query);

        /// 通过name获取单条数据([get]/api/test/{name:string},以path参数传参,并约束参数类型为string)
        [HttpGet("{name:string}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Get([HashKey] [CacheKey(0)] string name);

        /// 通过id获取单条数据([get]/api/test/{id:long},以path参数传参,并约束参数类型为long)
        [HttpGet("{id:long}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("id:{0}")]
        Task<TestOut> GetById([HashKey] [CacheKey(0)] long id);

        ///更新部分数据，使用patch请求 ([patch]/api/test)
        [HttpPatch]
        Task<string> UpdatePart(TestInput input);
    }

```

## 服务的实现

一般地,开发者应当将定义服务的接口和服务的实现分开定义在不同的程序集。应用服务接口程序集可以被打包成Nuget包或是以项目的形式被其他微服务应用引用，这样其他微服务就可以通过rpc代理的方式与该微服务应用进行通信。更多RPC通信方面的文档[请参考](rpc)。

一个服务接口可以有一个或多个实现类。只有应用接口在当前微服务应用中存在实现类,该微服务应用接口对应的服务条目才会生成相应的服务路由，并将服务路由信息注册到服务注册中心,同时其他微服务应用的实例会订阅到微服务集群的路由信息。

应用接口如果存在多个实现类的情况下,那么应用接口的实现类,需要通过`ServiceKeyAttribute`特性进行标识。`ServiceKeyAttribute`存在两个参数(属性)。


| 属性名称 | 说明   |  备注 |
|:---------|:-----|:-----| 
| name | 服务实现类的名称 | 通过webapi请求时,通过请求头`serviceKey`进行设置;rpc通信中,可以通过`ICurrentServiceKey`的实例调整要请求的应用接口实现。 |
| weight | 权重 | 如果通信过程中,未指定`serviceKey`,那么,会请求权重最高的应用接口的实现类 |

实例:

```csharp
/// 应用服务接口(如:可定义在ITestApplication.csproj项目)
[ServiceRoute]
public interface ITestAppService
{
    Task<string> Create(TestInput input);
   // 其他服务条目方法略
}


//------------------------------------//
/// 应用服务实例类1 (如:可定义在TestApplication.csproj项目)
[ServiceKey("v1", 3)]
public class TestAppService : ITestAppService
{
  public Task<string> Create(TestInput input)
  {
      return Task.FromResult("create v1")
  }
  // 其他接口实现方法略
}

//------------------------------------//
/// 应用服务实例类2  (如:可定义在TestApplication.csproj项目)
[ServiceKey("v2", 1)]
public class TestV2AppService : ITestAppService
{
  public Task<string> Create(TestInput input)
  {
      return Task.FromResult("create v2")
  }
   // 其他接口实现方法略
}

```

生成的swagger文档如下:

![appservice-and-serviceentry1.jpg](/assets/imgs/appservice-and-serviceentry1.jpg)


在rpc通信过程中,可以通过`IServiceKeyExecutor`的实例设置要请求的应用接口的`serviceKey`。

```csharp
private readonly IServiceKeyExecutor _serviceKeyExecutor;

public TestProxyAppService(ITestAppService testAppService,
    IServiceKeyExecutor serviceKeyExecutor)
{
    _testAppService = testAppService;
    _serviceKeyExecutor = serviceKeyExecutor;
}

public async Task<string> CreateProxy(TestInput testInput)
{
   return await _serviceKeyExecutor.Execute(() => _testAppService.Create(testInput), "v2");
}

```