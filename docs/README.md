---
home: true
heroText: Silky框架
heroImage: /assets/logo/logo.svg
tagline:  基于.net平台的微服务开发框架
actionText: 快速上手 →
actionLink: /silky/index
features:
- title: 安全
  details: 微服务主机不允许直接与集群外部通信；集群内部通过token进行验证；rpc通信支持ssl加密。

- title: 稳定
  details: 使用.net平台提供的主机托管应用服务,保证服务能够稳定的运行。

- title: 高性能
  details: 基于高性能的通信框架dotnetty/SpanNetty实现的rpc通信框架; rpc通信过程中支持缓存拦截。

- title: 易扩展
  details: Silky基于模块化设计,开发者可以很方便的扩展自定义模块; silky采用非侵入的设计,很方便与第三方组件整合。

- title: 使用方便
  details: silky只需要简单的配置和一句代码即可构建一个主机托管silky服务。

- title: 分布式事务
  details: Silky通过缓存拦截的方式实现TCC分布式事务。

footer: MIT Licensed | Copyright © 2021-present Liuhll
---

## 简单、方便的构建您的微服务

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<NormHostModule>()
            ;
    }
}
```

## 加入我们
- QQ群： 934306776

  ![qq-group.jpg](/assets/imgs/qq-group.jpg)