---
home: true
heroText: Silky框架
heroImage: /assets/logo/logo.svg
tagline:  基于.net平台的微服务开发框架
actionText: 快速上手 →
actionLink: /silky/index
features:
- title: RPC通信
  details: 基于Dotnetty实现的面向接口代理的高性能RPC调用

- title: 负载均衡
  details: 内置轮询、随机、哈希一致性等负载均衡算法

- title: 服务自动注册和发现
  details: 支持Zookeeper、Consul、Nacos作为服务注册中心,服务实例上下线实时感知

- title: 缓存拦截
  details: RPC通信过程中,支持缓存拦截,提高通信性能

- title: 分布式事务
  details: 通过拦截器和todo日志实现TCC分布式事务,保证数据的最终一致性

- title: 高度可扩展
  details: 可方便的替换silky框架提供的组件(例如:底层通信框架或是服务注册中心等);也可方便的与第三方组件整合

- title: 链路跟踪
  details: 通过SkyApm实现通信过程的链路跟踪

- title: 在线文档
  details: 通过swagger生成webapi在线文档

- title: 控制台
  details: 通过查看和管理微服务集群的控制台

footer: MIT Licensed | Copyright © 2021-present Liuhll
---

## 足够简单、方便的构建您的微服务应用

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
                .ConfigureSilkyGeneralHostDefaults();
            
    }
}
```

## 在线示例

- [Hero权限管理系统(https://hero.silky-fk.com)](https://hero.silky-fk.com)

## 加入我们

- QQ群： 934306776

  ![qq-group.jpg](/assets/imgs/qq-group.jpg)