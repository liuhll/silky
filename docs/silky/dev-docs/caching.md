---
title: 缓存
lang: zh-cn
---

## 缓存拦截

在silky框架中,为提高rpc通信性能，使用缓存拦截的设计,减少网络io操作,提高分布式应用的性能。

要想使用缓存拦截,必须要在将服务治理属性的`Governance.CacheEnabled`设置为`true`(该属性的缺省值为`true`)。如果将该属性值被设置为`false`，那么在rpc通信过程中,缓存拦截将无效。

缓存拦截在应用服务接口方法上通过缓存拦截特性进行设置,在silky框架中,存在如下三中类型的缓存特性，分别对数据缓存进行新增、更新、删除。

### 设置缓存特性--GetCachingInterceptAttribute

在rpc通信过程中,如果通过缓存的`Key`在分布式缓存中能够从分布式缓存中命中相应的缓存数据,那么就就直接从分布式缓存中间件中获取数据,并返回给服务调用者。如果没有命中缓存数据,那么就会通过网络通信的方式从服务提供者获取数据,并且将返回的数据新增到缓存中间件,在下次rpc通信过程中,就可以直接从缓存中间件中获取数据,从而达到减少io操作,提高分布式应用的性能。

设置缓存特性方式如下所示:

```csharp
// 在应用服务接口方法中通过`GetCachingIntercept`设置缓存拦截
[GetCachingIntercept("name:{0}")]
Task<TestOut> Create(TestInput input);


// 通过输入参数对模板参数的占位符进行设值 
public class TestInput  
{
    [CacheKey(0)] 
    [HashKey] 
    [Required(ErrorMessage = "名称不允许为空")]
    public string Name { get; set; }

    [Required(ErrorMessage = "地址不允许为空")]
    public string Address { get; set; }

    [Phone(ErrorMessage = "手机号码格式不正确")] 
    public string Phone { get; set; }
    
}
```
`GetCachingInterceptAttribute`特性存在一个参数--`keyTemplete`，该参数是一个字符串类型,可以为该字符串指定字符串占位符,索引从0开始计算，占位符的值通过`CacheKeyAttribute`在输入参数中指定。如果是简单类型的参数,直接在参数名前面通过`[CacheKey(keyTempleteIndex)]`(`keyTempleteIndex`指占位符的索引)指定,如果是参数类型是一个复杂参数,则通过对属性的特性进行标识。实际上生成的缓存的`key`会根据`keyTemplete`和对输入参数设置的`[CacheKey(keyTempleteIndex)]`的值以及返回结果类型(`ReturnType`)共同确认。

除此之外，`GetCachingInterceptAttribute`特性还存在一个`OnlyCurrentUserData`属性，如果当前缓存数据只与当前登录用户相关,那么需要将`OnlyCurrentUserData`属性值设置为`true`,生成的缓存的`key`会加上将当前用户id。

::: warning

需要注意的是,如果`OnlyCurrentUserData`属性值设置为`true`,需要确保,当前接口需要用户登录认证。

:::

### 更新缓存特性--UpdateCachingInterceptAttribute

在rpc通信过程中，对数据进行更新操作后,为保证缓存一致性,同时需要对缓存数据进行更新操作。开发者可以通过`UpdateCachingInterceptAttribute`特性对缓存数据进行更新。`UpdateCachingInterceptAttribute`特性的参数与属性与`GetCachingInterceptAttribute`一致,用法基本上也一致。

与被`GetCachingInterceptAttribute`特性标注的应用服务接口不同的是,在rpc通信过程中,被`UpdateCachingInterceptAttribute`特性标识的方法都会得到执行,只是将服务提供者返回的结果更新到缓存服务中。被更新的缓存数据与生成的缓存的`key`以及返回结果类型(`ReturnType`)有关。

```csharp
// 在应用服务接口方法中通过`GetCachingIntercept`设置缓存拦截
[UpdateCachingIntercept("name:{0}")]
Task<TestOut> Update(UpdateTestInput input);

// 通过输入参数对模板参数的占位符进行设值 
public class UpdateTestInput  
{
    public long Id { get; set; }

    [CacheKey(0)] 
    [HashKey] 
    [Required(ErrorMessage = "名称不允许为空")]
    public string Name { get; set; }

    [Required(ErrorMessage = "地址不允许为空")]
    public string Address { get; set; }

    [Phone(ErrorMessage = "手机号码格式不正确")] 
    public string Phone { get; set; }
    
}
```


### 删除缓存特性--RemoveCachingInterceptAttribute

在rpc通信过程中，对数据进行删除操作后,为保证缓存一致性,同时需要对缓存数据进行删除操作。开发者可以通过`RemoveCachingInterceptAttribute`特性对缓存数据进行移除操作。

`RemoveCachingInterceptAttribute`特性除了指定`keyTempalte`模板参数之外,还需要指定缓存名称(`CacheName`),一般地,在缓存拦截中,`CacheName`与要缓存数据(即:`ReturnType`)的类的完全限定名一致。

```csharp
[RemoveCachingIntercept("ITestApplication.Test.Dtos.TestOut", "name:{0}")]
[Transaction]
Task<string> Delete([CacheKey(0)] string name);
```

## 分布式缓存接口

silky框架提供分布式缓存接口`IDistributedCache<T>`,通过分布式缓存接口实现对缓存数据的增删改查。其中,泛型`T`指的是缓存数据的类型。在分布式缓存中,`T`与应用服务接口方法定义的返回值类型一致。

分布式缓存接口可以通过构造器注入,在使用分布式缓存接口是,必须要指定泛型参数`T`。

```csharp
public class TestAppService : ITestAppService
{

    private readonly IDistributedCache<TestOut> _distributedCache;
    public TestAppService(IDistributedCache<TestOut> distributedCache)  
    {
        // 通过构造器注入分布式缓存接口后,可以通过该接口实现对该缓存数据的增删改查。
        _distributedCache = distributedCache;
    }
}
```

### 缓存数据的一致性

缓存一致性的概念: 缓存一致性的本质是数据一致性。换句话说,就是在缓存服务中的数据与数据库中存储的数据要保证数据一致性。开发者在开发过程中,要特别注意对缓存数据的一致性。也就是说,如果对某个类型的数据进行缓存,那么，对其进行更新、删除操作时,需要同时对缓存服务中的缓存数据进行更新或是、删除操作。

在rpc通信过程中,使用缓存拦截,同一数据的缓存依据可能会不同(设置的`KeyTemplate`,例如:缓存依据可能会根据`Id`、`Name`、`Code`分别进行缓存),从而产生不同的缓存数据,但是在对数据进行更新、删除操作时,由于无法通过`RemoveCachingInterceptAttribute`特性一次性删除该类型数据的所有缓存数据,这个时候，在实现业务代码过程中,就需要通过分布式缓存接口`IDistributedCache<T>`实现缓存数据的更新、删除操作。

缓存数据的更新、命中如下图所示:

![caching1.png](/assets/imgs/caching1.png)

![caching2.png](/assets/imgs/caching2.png)

## 使用redis作为缓存中间件

silky框架默认使用`MemoryCaching`作为缓存,但是如果开发者需要将微服务集群部署到多台服务器,那么您需要使用`redis`服务作为缓存服务。

silky使用redis作为分布式缓存服务非常简单,您只要提供一个redis服务(集群),通过配置文件就可以将缓存中间件替换为redis服务。

配置如下所示:

```yml
distributedCache:
  redis:
    isEnabled: true
    configuration: 127.0.0.1:6379,defaultDatabase=0 //redis缓存链接配置
```