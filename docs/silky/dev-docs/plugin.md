---
title: 插件
lang: zh-cn
---

## 以插件的方式扫描添加应用服务

一般地,如果需要主机托管应用的话,需要主机所在的应用程序集引用应用服务的所在的程序集;

例如主机的项目程序集如下所示:

```xml

<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net7.0</TargetFramework>
        <OutputType>Exe</OutputType>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\Silky.Agent.Host\Silky.Agent.Host.csproj" />
        <ProjectReference Include="..\TestApplication\TestApplication.csproj" /> <!--应用服务程序集-->
    </ItemGroup>
</Project>

```

除此之外,我们可以通过插件的方式指定应用服务所在的目录,这样我们无需直接引用应用服务程序集,通过扫描指定的目录实现对应用服务的解析。

1. 通过配置的方式指定应用服务所在的目录

在配置文件`appsettings.yaml`添加如下配置：

```yaml
plugInSource:
  appServicePlugIns:
    - folder: ../../../TestApplication/bin/Debug/net7.0
      searchOption: TopDirectoryOnly
```

2. 通过应用服务启动配置项指定应用服务所在的目录

在构建主机时,我们可以通过代码的方式对`SilkyApplicationCreationOptions`的属性`AppServicePlugInSources`添加应用服务的目录;

```csharp

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host
                    .CreateDefaultBuilder(args)
                    .ConfigureSilkyGeneralHostDefaults(options =>
                    {
                        // ...其他配置项
                        options.AppServicePlugInSources.Add(new ServicePlugInOption()
                        {
                            Folder = "../../../TestApplication/bin/Debug/net7.0"
                        });

                    })
                ;

            return hostBuilder;
        }

```

## 以插件的方式添加模块

我们知道,在silky的模块系统中,模块具有依赖关系,我们通过[启动模块](modularity.html#构建主机时指定启动模块)来确定模块与模块的依赖关系以及解析出系统所依赖的所有模块,也就是说,只有直接或是间接的的对模块进行依赖,才可以被系统解析到并在应用启动时被加载和执行。

silky框架提供的注册silky服务已经默认指定了启动模块,所以,一般地我们无需修改启动模块，这个时候如果我们需要添加额外的业务模块,最好的方式就是以插件的方式添加模块。添加模块插件的方式与添加应用服务的方式雷同;

1. 通过配置的方式指定模块类

在配置文件`appsettings.yaml`添加如下配置：

```yaml
plugInSource:
  modulePlugIn:
    types:
    - TestApplication.TestModule,TestApplication
```

2. 通过应用服务启动配置项指定模块类

在构建主机时,我们可以通过代码的方式对`SilkyApplicationCreationOptions`的属性`ModulePlugInSources`添加模块类；

```csharp
 private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host
                    .CreateDefaultBuilder(args)
                    .ConfigureSilkyGeneralHostDefaults(options =>
                    {
                        // ...其他配置项
                          options.ModulePlugInSources.Add(new TypePlugInSource("TestApplication.TestModule,TestApplication"));

                    })
                ;

            return hostBuilder;
        }

```