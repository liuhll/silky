---
title: 服务引擎
lang: zh-cn
---

## 构建服务引擎

在[注册Silky微服务应用](/source/startup/host.html#注册silky微服务应用)一节中,我们了解到在`ConfigureServices`阶段,通过`IServiceCollection`的扩展方法`AddSilkyServices<T>()`除了注册必要的服务之外,更主要的是构建了服务引擎(`IEngine`)。



## 服务引擎的作用
