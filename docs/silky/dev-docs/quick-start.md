---
title: 快速开始
lang: zh-cn
---

## 必要前提

1. (**必须**) 安装了.net core3.1 或是 .net5 sdk。

2. (**必须**) 您可以使用visual studio 或是rider作为开发工具。 

3. (**必须**) 您必须准备一个可用的`zookeeper`服务作为服务注册中心。

4. (**必须**) 您必须准备一个可用的`redis`服务作为分布式锁服务。

5. (**可选**) 你可以选择`redis`服务作为分布式缓存服务。

## 使用泛型主机托管普通应用

### 构建业务微服务模块主机

**1. 新增一个控制台应用**

![quick-start1.png](/assets/imgs/quick-start1.png)

**2. 使用nuget安装`Silky.NormHost`包**

![quick-start2.png](/assets/imgs/quick-start2.png)

**3. 在`Main`方法中构建泛型主机并注册Silky服务,指定启动模块**

```csharp
namespace Silky.SampleHost
{
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;
    class Program
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
}
```

**4. 新增配置文件**

silky支持通过`json`或是`yml`格式进行配置。您可以通过`appsettings.yml`为公共配置项指定配置信息,也可以通过新增`appsettings.${ENVIRONMENT}.yml`文件为指定的环境配置信息。

一般地,您必须要指定rpc通信的`token`,服务注册中心地址,分布式锁服务地址,数据库连接等配置项。如果您使用redis作为缓存服务,那么您还需要将`distributedCache.redis.isEnabled`配置项设置为`true`,并给出redis服务缓存的地址。

配置信息如下所示:

```yaml
rpc:
  host: 0.0.0.0
  Port: 2201
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
governance:
  timeoutMillSeconds: 0
registrycenter:
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186
  registryCenterType: Zookeeper
distributedCache:
  redis:
    isEnabled: true
    configuration: 127.0.0.1:6379,defaultDatabase=0
lock:
  lockRedisConnection: 127.0.0.1:6379,defaultDatabase=1
connectionStrings:
  default: server=127.0.0.1;port=3306;database=account;uid=root;pwd=qwe!P4ss;
```

将配置文件属性的**复制到输出目录**,设置为: *始终复制* 或是 *如果较新则复制*。

![quick-start3.png](/assets/imgs/quick-start3.png)

### 业务微服务模块的其他项目

完成主机项目构建后,您可以新增**应用接口层**、**应用层**、**领域层**、**数据访问层**等其他项目,更多内容请参考[微服务架构](#)节点。

一般地,应用层依赖应用接口层,并引用领域层完成核心业务逻辑。领域层可以引用应用接口层实现对`dto`对象的使用。

一个典型的微服务模块的划分与传统的`DDD`领域模型的应用划分基本一致。只是需要将应用接口单独的抽象为一个程序集，方便被其他微服务模块或是网关引用，其他微服务模块可以通过应用接口生成rpc代理,与该微服务模块进行通信。

一个典型的微服务模块的项目结构如下所示:

![quick-start4.png](/assets/imgs/quick-start4.png)

主机要实现微服务应用的托管,他必须要引用应用服务的实现(即:对**应用层**的引用)。

![quick-start5.png](/assets/imgs/quick-start5.png)

一般地,我们需要在应用接口层对[服务路由](#)、[缓存拦截](#)、[服务条目治理](#)、[分布式事务](#)等进行特性配置，所以还还需要引用:`Silky.Rpc`和`Silky.Transaction`包。

![quick-start6.png](/assets/imgs/quick-start6.png)

应用层如果使用分布式事务的话,那么还需要对`Silky.Transaction.Tcc`包引用。

![quick-start6.1.png](/assets/imgs/quick-start6.1.png)


至此,一个普通的业务微服务模块就创建成功了,您可以通过引用该微服务的应用接口层的项目或是将其打包成nuget包,将其(应用接口层项目)安装到其他微服务模块,这样,其他微服务模块就可以通过应用接口生成的代理通过rpc与该服务模块进行通信。

## 使用web主机托管网关应用

**1. 新建一个web应用**

![quick-start7.png](/assets/imgs/quick-start7.png)

**2. 使用nuget安装`Silky.WebHost`包**

![quick-start8.png](/assets/imgs/quick-start8.png)

**3. 在`Main`方法中构建Web主机并注册Silky服务,指定启动模块和`Startup`**

```csharp
namespace Silky.Sample.Gateway
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .RegisterSilkyServices<WebHostModule>()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
```

**4. 在`Startup`中配置swagger文档以及设置使用silky请求管道**

```csharp
// 服务配置;注册Swagger文档服务
public void ConfigureServices(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Silky Gateway", Version = "v1" });
        c.MultipleServiceKey();
        var applicationAssemblies = EngineContext.Current.TypeFinder.GetAssemblies()
            .Where(p => p.FullName.Contains("Application") || p.FullName.Contains("Domain"));
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

//中间件配置;配置swagger文档中间件、以及配置启用silky请求管道
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment() || env.EnvironmentName == "ContainerDev")
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Silky Gateway Demo v1"));
    }
    app.ConfigureSilkyRequestPipeline(); // 会自动使用配置了的silky中间件
}

```


**4. 新增配置文件**

一般地,您必须要网关项目中指定rpc通信的`token`,服务注册中心地址,分布式锁服务地址。如果您使用redis作为缓存服务,那么您还需要将`distributedCache.redis.isEnabled`配置项设置为`true`,并给出redis服务缓存的地址。

一个典型的网关服务配置如下所示:

```yaml
rpc:
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
registrycenter:
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186
  registryCenterType: Zookeeper
distributedCache:
  redis:
    isEnabled: true
    configuration: 127.0.0.1:6379,defaultDatabase=0
lock:
  lockRedisConnection: 127.0.0.1:6379,defaultDatabase=1
```

**5. 引用其他微服务模块的应用接口层**

开发者可以选择通过nuget包的方式或是直接项目引用的方式来使用其他微服务模块的应用接口层。**http请求**到达网关后,就可以根据指定的应用接口生成代理与集群内部进行通信。

![quick-start9.png](/assets/imgs/quick-start9.png)

接下来,您就可以在各个微服务模块中的应用接口层定义服务接口,并实现相应的业务逻辑。