---
title: 应用接口和服务条目
lang: zh-cn
---

## 应用接口的定义

服务应用接口是微服务定义webAPI的基本单位，可以被其他微服务应用引用，其他微服务可以通过rpc通信与该微服务进行通信。应用接口如果被网关应用引用,网关可以通过服务应用接口生成swagger文档。

通过`ServiceRouteAttribute`特性对一个接口进行标识即可成为一个服务应用接口。

例如:

```csharp

[ServiceRoute]
public interface ITestAppService
{
}

```

## 服务路由特性

开发者对应用接口标识路由(`ServiceRouteAttribute`)时,可以通过路由特性的属性对生成的路由模板、应用接口请求头是否支持`serviceKey`进行配置。

| 属性名称 | 说明   |  缺省值  | 
|:---------|:----- |:--------| 
| template | 在对服务应用接口标识为服务路由时,可以通过`[ServiceRoute="template")`指定应用服务接口的路由模板。| api/{appservice} | 
| multipleServiceKey | 网关生成的webapi请求头是否支持`serviceKey`请求头 | false | 

## 服务条目

**服务条目(ServiceEntry)**: 服务应用接口中定义的每一个方法都会生成微服务集群的一个服务条目。对微服务应用本身而言,服务条目就相当于MVC中的`Action`，服务应用接口就相当于`Controller`。

### 根据服务条目生成webAPI

应用接口被网关引用后,会根据服务应用接口的路由模板和服务条目方法的Http动词特性指定的路由信息或是方法名称生成相应的webapi,服务条目生成的WebAPI支持restfulAPI风格。


服务条目生成webapi的规则为:

1. 禁止集群外部访问的服务条目(`[Governance(ProhibitExtranet = true)]`)不会生成webapi;

2. 可以通过`[ServiceRoute="template")]`为应用接口指定统一的路由模板;

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
    [ServiceRoute(multipleServiceKey: true)]
    public interface ITestAppService
    {
        /// <summary>
        ///  新增接口测试
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Create(TestInput input);

        Task<string> Update(TestInput input);

        [RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "name:{0}")]
        [Transaction]
        Task<string> Delete([CacheKey(0)] string name);

        [HttpGet]
        Task<string> Search([FromQuery] TestInput query);

        [HttpPost]
        string Form([FromForm] TestInput query);

        [HttpGet("{name:string}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("name:{0}")]
        Task<TestOut> Get([HashKey] [CacheKey(0)] string name);

        [HttpGet("{id:long}")]
        [Governance(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [GetCachingIntercept("id:{0}")]
        Task<TestOut> GetById([HashKey] [CacheKey(0)] long id);

        //[HttpPatch("patch")]
        [HttpPatch]
        [Governance(FallBackType = typeof(UpdatePartFallBack))]
        Task<string> UpdatePart(TestInput input);
    }

```

## 应用接口的实现

