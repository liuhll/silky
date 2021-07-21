---
title: 网关
lang: zh-cn
---

## 网关概述

不同的微服务一般会有不同的网络地址，而外部客户端可能需要调用多个服务的接口才能完成一个业务需求，如果让客户端直接与各个微服务通信，会有以下的问题：

- 客户端会多次请求不同的微服务，增加了客户端的复杂性
- 存在跨域请求，在一定场景下处理相对复杂
- 认证复杂，每个服务都需要独立认证
- 难以重构，随着项目的迭代，可能需要重新划分微服务。例如，可能将多个服务合并成一个或者将一个服务拆分成多个。如果客户端直接与微服务通信，那么重构将会很难实施
- 某些微服务可能使用了防火墙 / 浏览器不友好的协议，直接访问会有一定的困难

以上这些问题可以借助**网关**解决。

**网关**是介于客户端和服务器端之间的中间层，所有的外部请求都会先经过 网关这一层。也就是说，API 的实现方面更多的考虑业务逻辑，而安全、性能、监控可以交由 网关来做，这样既提高业务灵活性又不缺安全性。

![gateway1.png](/assets/imgs/gateway1.png)

## silky框架网关

silky框架的普通微服务被设计为使用.net的[通用主机](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0)进行托管,并自定义rpc端口与其他微服务应用进行通信(rpc端口号缺省值为:`2200`)。为保证微服务应用的安全性,通过`rpc.token`的设计方式,避免了集群外部rpc端口号直接与微服务内部进行通信。

那么,服务外部(前端)是如何与微服务应用进行通信呢?

silky网关被设计为对微服务应用集群的聚合，需要安装每个微服务应用的应用接口项目(包)，前端通过http请求到达网关,silky中间件通过`webapi`+`http方法`在路由表中找到应用服务Id,然后通过rpc与服务提供者进行通信,并将返回结果封装后返回给前端。

在网关应用,开发者可以增加或自定义中间件实现接口的统一认证与授权,服务限流,流量监控等功能。

## 构建网关应用

1. 通过nuget安装`Silky.WebHost`包，在主函数中注册和构建主机

```csharp
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<WebHostModule>()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
```

2. 在`Startup`类中添加**swagger在线文档**,和配置silky请求管道(自动注册一系列的silky中间件)。

```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Silky Gateway Demo", Version = "v1" });
                c.MultipleServiceKey();
                var applicationAssemblies = EngineContext.Current.TypeFinder.GetAssemblies()
                    .Where(p => p.FullName.Contains("Application"));
                foreach (var applicationAssembly in applicationAssemblies)
                {
                    var xmlFile = $"{applicationAssembly.GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                }

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Silky Gateway Demo v1"));
            }
            app.ConfigureSilkyRequestPipeline();
        }
```

3. 通过项目引用的方式或是nuget包的方式安装各个微服务应用的应用服务接口层(包)

silky通过引用各个微服务应用的应用接口,可以为每个应用服务接口生成webapi,开发者可以通过swagger在线文档进行开发调式。