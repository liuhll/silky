---
title: 链路跟踪
lang: zh-cn
---

## 链路跟踪的概念

在分布式系统，尤其是微服务系统中，一次外部请求往往需要内部多个模块，多个中间件，多台机器的相互调用才能完成。在这一系列的调用中，可能有些是串行的，而有些是并行的。在这种情况下，我们如何才能确定这整个请求调用了哪些应用？哪些模块？哪些节点？以及它们的先后顺序和各部分的性能如何呢？

链路追踪是分布式系统下的一个概念，它的目的就是要解决上面所提出的问题，也就是将一次分布式请求还原成调用链路，将一次分布式请求的调用情况集中展示，比如，各个服务节点上的耗时、请求具体到达哪台机器上、每个服务节点的请求状态等等。

## 搭建skywalking服务

搭建skywalking需要用到三个镜像:
-  elasticsearch：用来存储数据
-  skywalking-oap-server：Skywalking服务器
-  skywalking-ui ：Skywalking的UI界面

silky框架已经将上述三个服务编排到[docker-compose.skywalking.yml](https://raw.githubusercontent.com/liuhll/silky/main/framework/test/docker-compose/infrastr/docker-compose.skywalking.yml)文件,开发者可以进入目录 *framework/test/docker-compose/infrastr* 获取该编排文件,并通过如下命令部署skywalking服务;

```shell

docker-compose -f docker-compose.skywalking.yml up -d

```

当服务启动后，oap默认端口为11800,SkyWalking UI的默认地址是8080，开发者可以打开 *http://127.0.0.1:8080*即可打开SkyWalking UI,通过**普通服务/服务/Trace**查看某个请求的调用链路；


## Silky链路跟踪

1. 搭建skywalking服务
   
开发者可以根据[上述文档](#搭建skywalking服务) 搭建skywalking服务;

2. 添加对SkyApm服务的引用

在启动项目中,添加一个`ConfigureService`类并实现`IConfigureService`接口,通过`IServiceCollection`添加对SkyApm的引用,开发者可以通过[依赖注入](dependency-injection.html#将对象注入到ioc容器的方式)。

```csharp
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSilkySkyApm(); //添加对SkyApm服务的引用

            // 添加其他服务注入
        }
    }
```

3. 添加配置

在启动项目中添加`skyapm.json`配置文件,添加如下配置:

```json
{
  "SkyWalking": {
    "ServiceName": "DemoHost", // 该服务主机的名称 
    "Namespace": "",
    "HeaderVersions": [
      "sw8"
    ],
    "Sampling": {
      "SamplePer3Secs": -1,
      "Percentage": -1.0
    },
    "Logging": {
      "Level": "Information",
      "FilePath": "logs/skyapm-{Date}.log"
    },
    "Transport": {
      "Interval": 3000,
      "ProtocolVersion": "v8",
      "QueueSize": 30000,
      "BatchSize": 3000,
      "gRPC": {
        "Servers": "127.0.0.1:11800", // skywalking服务地址
        "Timeout": 10000,
        "ConnectTimeout": 10000,
        "ReportTimeout": 600000
      }
    }
  }
}
```

其中,最重要的配置项`SkyWalking:ServiceName`(主机名称)和`Transport:gRPC:Servers`(skywalking服务地址)最为重要,开发者可以根据实际情况进行调整;

4. 通过silky-ui地址查看服务之间的调用链路

在调用silky webapi服务之后(例如:调用示例项目中的`/api/tcctest/test`webAPI后),打开silky-ui地址*http://127.0.0.1:8080*后,即可通过 **普通服务/服务/Trace** 查看该请求的调用链路

![skywalking1.png](/assets/imgs/skywalking1.png)