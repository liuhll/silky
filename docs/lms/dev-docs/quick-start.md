# 快速开始

## 必要前提

1. 安装了.net core3.1 或是 .net5 sdk。

2. 您可以使用visual studio 或是rider作为开发工具。 

3. 您必须准备一个可用的`zookeeper`服务作为服务注册中心。

4. 您必须准备一个可用的`redis`服务作为分布式锁服务。

## 使用泛型主机托管普通应用

### 构建业务微服务模块主机

1. 新增一个控制台应用

![quick-start1.png](/assets/imgs/quick-start1.png)

2. 使用nuget安装`Silky.Lms.NormHost`包

![quick-start2.png](/assets/imgs/quick-start2.png)

3. 在Main方法中构建泛型主机并注册Lms服务,指定启动模块

```csharp
namespace Lms.SampleHost
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
                    .RegisterLmsServices<NormHostModule>()
                ;
        }
    }
}
```

4. 新增配置文件

lms支持通过`json`或是`yml`格式进行配置。您可以通过`appsettings.yml`为公共配置项指定配置信息,也可以通过新增`appsettings.${ENVIRONMENT}.yml`文件为指定的环境配置信息。

一般地,您必须要指定rpc通信的token,服务注册中心地址,分布式锁服务地址,数据库连接等配置项。如果您使用redis作为缓存服务,那么您还需要将`distributedCache.redis.isEnabled`配置项设置为`true`,并给出redis服务缓存的地址。

配置信息如下所示:

```yaml
rpc:
  host: 0.0.0.0
  rpcPort: 2201
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
governance:
  executionTimeout: -1
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

一般地,我们需要在应用接口层对[服务路由](#)、[缓存拦截](#)、[服务条目治理](#)、[分布式事务](#)等进行特性配置，所以还还需要引用:`Silky.Lms.Rpc`和`Silky.Lms.Transaction`包。

![quick-start6.png](/assets/imgs/quick-start6.png)


至此,一个普通的业务微服务模块就创建成功了,您可以通过引用该微服务的应用接口层的项目或是将其打包成nuget包,将其(应用接口层项目)安装到其他微服务模块,这样,其他微服务模块就可以通过应用接口生成的代理通过rpc与该服务模块进行通信。

## 使用web主机托管网关应用