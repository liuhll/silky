---
title: rpc通信
lang: zh-cn
---

## 什么是RPC

RPC 全称 Remote Procedure Call——远程过程调用。是为了解决远程调用服务的一种技术，使得调用者像调用本地服务一样方便透明。简单的说，RPC就是从一台机器（客户端）上通过参数传递的方式调用另一台机器（服务器）上的一个函数或方法（可以统称为服务）并得到返回的结果。

RPC框架的目标就是让远程过程(服务)调用更加简单、透明，RPC框架负责屏蔽底层的传输方式(TCP或UDP)、序列化方式(XML/JSON/二进制)和通信细节。框架使用者只需要了解谁在设么位置提供了什么样的远程服务接口即可，开发者不需要关心底层通信细节和调用过程。

RPC通信有如下特点：

- RPC 会隐藏底层的通讯细节（不需要直接处理Socket通讯或Http通讯）
- RPC 是一个请求响应模型。客户端发起请求，服务器返回响应（类似于Http的工作方式）
- RPC 在使用形式上像调用本地函数（或方法）一样去调用远程的函数（或方法）。

一个RPC框架由三部分组成：

1. 服务提供者: 它运行在服务端,负责提供服务接口定义和服务实现类。

2. 服务发布者：它运行在RPC服务端,复杂将本地服务发布成远程服务,供其他消费者调用。

3. 本地服务代理: 它运行在RPC客服端,通过代理调用远程服务提供者,然后将结果进行封装给本地消费者。



RPC通信模型如下图所示:

![rpc1.jpg](/assets/imgs/rpc1.jpg)

## Silky的rpc框架实现

Silky框架使用rpc协议实现服务之间的通信。我们知道,要实现一个RPC框架,需要解决几个如下技术点:

1. 远程服务提供者：需要以某种形式提供服务调用相关的信息，包括但不限于服务连接口定义、数据接口、或者中间态的服务定义文件；在Silky框架中,服务提供者以服务应用接口的方式提供服务调用的相关信息，服务调用者通过引用服务提供者的应用服务接口层(项目、包)的方式获取远程服务调用的相关信息。

2. 远程代理对象：服务调用者调用的服务实际上是远程服务的本地代理，对于Silky框架而言，是通过`Autofac.Extras.DynamicProxy`组件实现的动态代理，将本地调用封装成远程服务调用,隐藏了底层的通信细节。

3. 通信：RPC框架于具体的通信协议无关；Silky框架使用dotnetty框架作为底层的通信框架。

4. 序列化：远程通信，需要将对象转换成二进制码流进行网络传输，不同的序列化框架，支持的数据类型/数据包大小、异常类型以及性能等都不同。Silky框架默认使用json格式作为序列化格式，也支持通过`MessagePack`或是`Protobuff`作为序列化方式。

## 如何使用

在开发过程中,我们一般需要将服务的定义(接口)和服务的实现(类)分别定义在两个不同的程序集，方便服务的接口被其他微服务应用引用，在Silky框架中,服务调用者(消费者)通过引用服务提供者的应用接口层(包),就可以通过动态代理代理机制为服务应用接口生成本地代理,通过该代理与应用服务提供者进行通信,并将执行的结果返回给消费者，步骤如下所述：

1. 将服务提供者的应用接口单独定义为一个应用程序集(包)。定义的应用接口需要通过`ServiceRouteAttribute`对服务应用接口的路由进行标识，服务提供者需要实现应用接口。关于应用接口的定义和实现[请查看](appservice-and-serviceentry)。

2. 服务调用者服务需要通过项目(或是通过nuget包安装)的方式引用服务提供者的应用接口所定义的项目(包)。

3. 开发者可以通过构造注入的方式使用应用服务接口，服务调用者就可以通过服务调用者的应用接口生成的动态代理与服务提供者进行通信。

```csharp
public class TestProxyAppService : ITestProxyAppService
{
    private readonly ITestAppService _testAppServiceProxy; // 应用提供者的应用接口，通过其生成服务调用者的本地动态代理
    private readonly ICurrentServiceKey _currentServiceKey;

    public TestProxyAppService(ITestAppService testAppService,
        ICurrentServiceKey currentServiceKey)
    {
        _testAppServiceProxy = testAppService;
        _currentServiceKey = currentServiceKey;
    }

    public async Task<TestOut> CreateProxy(TestInput testInput)
    {
        // _currentServiceKey.Change("v2");
        // 通过应用接口生成的本地动态代理与服务提供者进行rpc通信
        return await _testAppServiceProxy.Create(testInput);
    }
}

```

::: warning

在rpc通信过程中,通过指定的`ServiceKey`来指定服务提供者的应用接口的实现类,可以在服务调用前,通过`IServiceKeyExecutor`来指定该rpc通信的`serviceKey`的值。   

:::