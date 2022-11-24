<p align="center">
  <img height="200" src="./docs/.vuepress/public/assets/logo/logo.svg">
</p>

# Silky Microservice Framework
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![Commit](https://img.shields.io/github/last-commit/liuhll/silky)](https://img.shields.io/github/last-commit/liuhll/silky)
[![NuGet](https://img.shields.io/nuget/v/silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![MyGet (nightly builds)](https://img.shields.io/myget/silky-preview/vpre/Silky.Core.svg?style=flat-square)](https://www.myget.org/feed/Packages/silky-preview)
[![NuGet Download](https://img.shields.io/nuget/dt/Silky.Core.svg?style=flat-square)](https://www.nuget.org/packages/Silky.Core)
[![Bilibili](https://img.shields.io/badge/bilibili-silky__fk-fb7299?logo=bilibili)](https://space.bilibili.com/354560671/channel/seriesdetail?sid=2797330)
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2Fliuhll%2Fsilky&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)

<div align="center">

**English | [简体中文](./README.md)**

</div>

## Give a Star! ⭐️

If you liked this repo or if it helped you, please give a star ⭐️ for this repository. That will not only help strengthen our community but also improve the skills of developers to learn Silky framework 👍. Thank you very much.

## Project Introduction

The silky framework is designed to help developers quickly build a microservice application development framework through simple code and configuration under the .net platform. It provides two key capabilities of RPC communication and microservice governance. This means that the microservices developed using silky will have mutual remote discovery and communication capabilities. At the same time, using the rich service governance capabilities provided by silky, service governance demands such as service discovery, load balancing, and traffic scheduling can be realized. At the same time, silky is highly scalable, and users can customize their own implementation at almost any function point to change the default behavior of the framework to meet their business needs.

The silky microservice has the following advantages:

- out of the box
  - Simple and easy to use, use a general-purpose host or a web host to build (host) microservice applications.
  - Ease of use is high, and the interface-oriented proxy feature can realize local transparent calls.
  - Rich in functions, most of the microservice governance capabilities can be realized based on native libraries or lightweight extensions.

- Ultra-large-scale microservice cluster practice
  - A high-performance cross-process communication protocol, using the DotNetty communication framework to implement the RPC framework based on the interface proxy, providing high-performance proxy-based remote call capabilities, the service is based on the interface, and the underlying details of the remote call are shielded for developers.
  - In terms of address discovery and traffic management, it can easily support large-scale cluster instances.
  
- Enterprise-level microservice governance capabilities
  - Through the service governance implemented by Polly, the fault tolerance of the service is improved.
  - A variety of built-in load balancing strategies can intelligently perceive the health status of downstream nodes, significantly reduce call delays, and improve system throughput.
  - Supports multiple registration center services, real-time perception of service instances going online and offline.

- Guarantee of Data Consistency
  - Use TCC distributed transactions to ensure the final consistency of data.

## frame properties

### Service Engine

- Responsible for the initialization process of the silky host
- Responsible for module parsing, dependency management and loading
- Service registration and analysis

### Modular/Plug-in design
- There are dependencies between modules
- Support plug-in loading module
- Support plug-in loading application services

### RPC communication

- Use [Dotnetty](https://github.com/Azure/DotNetty) as the underlying communication component, use TCP as the communication protocol, and use long links to improve system throughput
- Interface-based dynamic proxy
- Support calling via template
- Support JSON encoding and decoding
- Support cache interception during RPC communication to improve communication performance
- RPC call monitoring

### Service Governance

- Automatic registration and discovery of services, intelligent perception of service instances going online and offline
- RPC call failed retry
- Supports load balancing routing methods such as polling, random routing, and hash consistency, intelligently perceives the health status of downstream nodes, significantly reduces call delays, and improves system throughput.
- Support HTTP current limit and RPC call current limit
- Support fuse protection, when non-friendly exceptions occur n times, turn on the fuse protection
- Support monitoring of RPC calls
- Service degradation, when the RPC call fails, call the method specified by `Fabllback` to achieve the purpose of service fault tolerance
- Prohibit services from external access through configuration support

### Build via .net host

- Use web hosting to build microservice applications
- Build microservice applications using a general-purpose host
- Build microservice applications with websocket capabilities
- Build gateway application
  

### Security Design

- The gateway performs identity authentication and authentication in a unified manner
- The rpc token is used to protect the RPC communication, ensuring that the external cannot directly access the rpc service
- RPC communication supports ssl encryption

### Various configuration methods

- Support Json format configuration files
- Support configuration files in Yaml format
- Support Apollo as a configuration service center
- Use environment variables

### Link Tracking

- HTTP requests
- RPC calls
- TCC distributed transactions
- Other (EFCore)...

### Support distributed transactions

- In the process of RPC communication, the final consistency of data is guaranteed through the TCC distributed framework
- Implemented using interceptor + TODO log
- Use Redis as TODO log storage warehouse

### Support websocket communication

- Build websocket services through [websocketsharp.core](https://www.nuget.org/packages/websocketsharp.core/) components
- Handshake and talk with the front end through the gateway proxy

## Getting Started

- Learn the Silky framework through [Developer Documentation](http://docs.silky-fk.com/silky/);
- Familiarize yourself with how to use the Silky framework to build a microservice application through [silky.samples project](http://docs.silky-fk.com/silky/dev-docs/quick-start.html);
- Familiarize yourself with the relevant configuration properties of the Silky framework through the [Configuration](http://docs.silky-fk.com/config/) document;
- Learn through the [silky-samples](https://github.com/liuhll/silky-samples) example project;
- Learn through Bilibili [silky framework teaching](https://space.bilibili.com/354560671/channel/seriesdetail?sid=2797330);

## Example project

### Silky.Hero authority management system

* project address
https://github.com/liuhll/silky.hero

* Demo address
https://hero.silky-fk.com/

* Account information (tenant silky)
   * Administrator account (password): admin(123qweR!)
   * Ordinary user: liuhll(123qweR!)
   * Other account password: 123qweR!

## Quick start

### Basic services

It is recommended to use `docker-compose` to install and deploy basic services.

1. Install and deploy Zookeeper, [docker-compose.zookeeper.yml](https://raw.githubusercontent.com/liuhll/silky/main/framework/test/docker-compose/infrastr/docker-compose.zookeeper.yml ) copy and keep it locally, and then install the Zookeeper service through the following command:

```shell
docker-compose -f docker-compose.zookeeper.ym up -d
```

2. Install and deploy redis cache service, set [docker-compose.redis.yml](https://raw.githubusercontent.com/liuhll/silky/main/framework/test/docker-compose/infrastr/docker-compose.redis .yml) and keep it locally, then install the redis service with the following command:

```shell
docker-compose -f docker-compose.redis.ym up -d
```

### Create Gateway

1. Create an **empty WebApplication** project named **Gateway**, install the `Silky.Agent.Host` package, and add the code to create a hosted gateway application host in the `Program.cs` class;

```csharp
using Gateway;

var hostBuilder = Host.CreateDefaultBuilder()
    .ConfigureSilkyGatewayDefaults(webHostBuilder => webHostBuilder.UseStartup<Startup>());
await hostBuilder.Build().RunAsync();

```

2. Add `Startup.cs` class and add the following code;

```csharp
namespace Gateway;

public class Startup
{
    public void ConfigureService(IServiceCollection services)
    {
        services.AddSilkyHttpServices()
            .AddRouting()
            .AddSwaggerDocuments()
            .AddMiniProfiler();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwaggerDocuments();
            app.UseMiniProfiler();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapSilkyRpcServices(); });
    }
}
```

3. Delete the `.json` configuration file, and add the `appsetiings.yaml` configuration file, and add the following configuration:

```yaml
rpc:
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
 
registrycenter:
  type: Zookeeper
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186
distributedCache:
  redis:
    isEnabled: true
    configuration: 127.0.0.1:6379,defaultDatabase=0
```

4. Run the gateway project, check the address where the http service runs (for example: the https port is 7160), then open the *https://127.0.0.1:7160/index.html* swagger online document through the browser; When the service is added to the application service, the swagger document does not have any interface:


![noexistservice.png](docs/.vuepress/public/assets/imgs/noexistservice.png)


### Business Microservices

1. Create a console project named **DemoHost**, install the `Silky.Agent.Host` package, and add the code to create a managed application host in the `Program.cs` class;

```csharp
using Microsoft.Extensions.Hosting;

var hostBuilder = Host.CreateDefaultBuilder().ConfigureSilkyGeneralHostDefaults();
await hostBuilder.Build().RunAsync();                      
```

2. Add `appsettings.yaml` configuration file and add the following configuration:

```yaml
rpc:
  token: ypjdYOzNd4FwENJiEARMLWwK0v7QUHPW
  port: 2200
 
registrycenter:
  type: Zookeeper
  connectionStrings: 127.0.0.1:2181,127.0.0.1:2182,127.0.0.1:2183;127.0.0.1:2184,127.0.0.1:2185,127.0.0.1:2186

distributedCache:
  redis:
    isEnabled: true
    configuration: 127.0.0.1:6379,defaultDatabase=0
```

3. Add a sample service, add a new folder **Hello**, and add `IHellAppService` interface:

```csharp
[ServiceRoute]
public interface IHelloAppService
{
    Task<string> SayHi([FromQuery]string name);
}
```

4. Add the `HellAppService` class and implement the `IHellAppService` interface:

```csharp
public class HelloAppService : IHelloAppService
{
    public Task<string> SayHi(string name)
    {
        return Task.FromResult($"Hello {name ?? "World"}");
    }
}
```

5. Run the **DemoHost** project, and refresh the Swagger online document through the browser, you can see the following interface, and you can debug the webapi online through the swagger document:

![helloservice.png](docs/.vuepress/public/assets/imgs/helloservice.png)

## How to call between services

1. By referring to the application interface class library of other microservice applications (other microservices can package the application interface into a nuget package, and install the nuget package of the application interface of other microservice applications through the nuget package), by constructing the injected interface , directly using the method defined by the interface, you can realize RPC communication with the service provider through the dynamic agent generated by the interface:

For example: Permission Manager [PermissionManager.cs](https://github.com/liuhll/silky.hero/blob) in [Silky.Hero](https://github.com/liuhll/silky.hero) project /main/services/Silky.Permission/src/Silky.Permission.Domain/Permission/PermissionManager.cs)

```csharp
public class PermissionManager : IPermissionManager, IScopedDependency
{
    private readonly IUserAppService _userAppService;
    private readonly IRoleAppService _roleAppService;

    public PermissionManager(IUserAppService userAppService,
        IRoleAppService roleAppService)
    {
        _userAppService = userAppService;
        _roleAppService = roleAppService;
    }

    public async Task<ICollection<string>> GetUserRoleNamesAsync(long userId)
    {
        var userRoleOutput = await _userAppService.GetRolesAsync(userId);
        return userRoleOutput.RoleNames;
    }

    public async Task<ICollection<long>> GetUserRoleIdsAsync(long userId)
    {
        var userRoleIds = await _userAppService.GetRoleIdsAsync(userId);
        return userRoleIds;
    }

    public async Task<ICollection<string>> GetRolePermissionsAsync(long roleId)
    {
        var rolePermissions = await _roleAppService.GetPermissionsAsync(roleId);
        return rolePermissions;
    }
}
```

2. Call the API provided by the interface `IInvokeTemplate` through the template to realize the remote service call. This interface supports routing to the specific service provider method through the service item Id or WebAPI;

For example: In the [Silky.Hero](https://github.com/liuhll/silky.hero) project, the gateway’s authority authentication processor [AuthorizationHandler](https://github.com/liuhll/silky.hero/ blob/main/services/Silky.Gateway/src/Silky.GatewayHost/Authorization/AuthorizationHandler.cs) Call the authorization service provided by the authorization application service through `IInvokeTemplate` to determine whether the currently requested interface has access rights:

```csharp

public class AuthorizationHandler : SilkyAuthorizationHandlerBase
{
    private readonly IInvokeTemplate _invokeTemplate;

    private const string CheckPermissionServiceEntryId =
        "Silky.Permission.Application.Contracts.Permission.IPermissionAppService.CheckPermissionAsync.permissionName_Get";

    private const string CheckRoleServiceEntryId =
        "Silky.Permission.Application.Contracts.Permission.IPermissionAppService.CheckRoleAsync.roleName_Get";

    public AuthorizationHandler(IInvokeTemplate invokeTemplate)
    {
        _invokeTemplate = invokeTemplate;
    }

    protected override async Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context,
        HttpContext httpContext,
        IAuthorizationRequirement requirement)
    {
        if (requirement is PermissionRequirement permissionRequirement)
        {
            if (EngineContext.Current.HostEnvironment.EnvironmentName == SilkyHeroConsts.DemoEnvironment &&
                httpContext.Request.Method != "GET")
            {
                throw new UserFriendlyException("The demo environment does not allow modification of data");
            }

            var serviceEntryDescriptor = httpContext.GetServiceEntryDescriptor();
            if (serviceEntryDescriptor.GetMetadata<bool>("IsSilkyAppService"))
            {
                // todo 
                return true;
            }

            return await _invokeTemplate.InvokeForObjectByServiceEntryId<bool>(CheckPermissionServiceEntryId,
                permissionRequirement.PermissionName);
        }

        return true;
    }

    protected override async Task<bool> PipelineAsync(AuthorizationHandlerContext context, HttpContext httpContext)
    {
        var serviceEntryDescriptor = httpContext.GetServiceEntryDescriptor();
        var roles = serviceEntryDescriptor
            .AuthorizeData
            .Where(p => !p.Roles.IsNullOrEmpty())
            .SelectMany(p => p.Roles?.Split(","))
            .ToList();
        foreach (var role in roles)
        {
            if (!await _invokeTemplate.InvokeForObjectByServiceEntryId<bool>(CheckRoleServiceEntryId, role))
            {
                return false;
            }
        }

        return true;
    }
}
```

> Remarks:
>
> The advantage of using the template call method is that there is no need to refer to the application interface defined by other microservice applications between microservice applications, and the application and application are completely decoupled and independent of each other; the disadvantage is that it does not support the usage scenarios of distributed transactions;

## Quickly create applications through project templates

silky provides a template `silky.app` template to quickly create applications, and developers can use modules to quickly create silky microservice applications after installing the template.

```pwsh

> dotnet new --install Silky.App.Template
```

Use project templates to create microservice applications.

```pwsh

PS> dotnet new silky.app -h
Silky App (C#)
Author: Liuhll

Usage:
  dotnet new silky.app [options] [template options]

Options:
  -n, --name <name> The name of the output being created. If no name is specified, the name of the output directory is used.
  -o, --output <output> Location to place generated output.
  --dry-run If running the given command line would result in template creation, show a summary of what would happen.
  --force Force generation of content (even if it changes existing files).
  --no-update-check Disable checking for template bundle updates when instantiating templates.
  --project <project> Project to apply to context evaluation.
  -lang, --language <C#> Specifies the template language to instantiate.
  --type <project> Specifies the type of template to instantiate.

Template options:
  -t, --param:type <param:type> Set the silky host type, optional values: webhost, generalhost ,wshost, gateway
                                 type: string
                                 Default: generalhost
  -do, --dockersupport Add docker support for Silky
                                 Type: bool
                                 Default: true
  -r, --rpcport <rpcport> Set the port for rpc listening
                                 Type: int
                                 Default: 2200
  -in, --infrastr only include basic service orchestration files
                                 Type: bool
                                 Default: false
  -e, --env <env> Set dotnet env
                                 type: string
                                 Default: Development
  -m, --module Is it a module project
                                 Type: bool
                                 Default: false
  -p:i, --includeinfr Whether to include the basic orchestration service.
                                 Type: bool
```

Example:

```pwsh

# create gateway
> dotnet new silky.app -t gateway -n Silky.Gateway

# Create a business microservice
> dotnet new silky.app -t generalhost -n Silky.Demo
```

## Contribute
- One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting Pull Request code changes.
