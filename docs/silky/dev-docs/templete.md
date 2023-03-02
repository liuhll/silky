---
title: 脚手架
lang: zh-cn
---

## 模板简介

使用 `dotnet new` 命令可以创建[模板](https://docs.microsoft.com/zh-cn/dotnet/core/tools/custom-templates),也就是我们常说的脚手架工具。silky框架也提供了模板,开发者可以选择Silky提供的的模板快速构建微服务应用。

## 通过项目模板快速创建应用[Silky.App.Template](https://www.nuget.org/packages/Silky.App.Template/)

silky提供了模板silky.app模板可以快速的创建应用，开发者可以在安装模板后使用模块快速创建silky微服务应用。

1. 安装 **Silky.App.Template** 模板

```powershell
dotnet new --install Silky.App.Template
```

2. 创建微服务应用

通过如下命令创建一个新的微服务应用：

```powershell
dotnet new --install Silky.App.Template
```

**Silky.App.Template** 模板参数:

```shell
PS> dotnet new silky.app -h
Silky App (C#)
作者: Liuhll

Usage:
  dotnet new silky.app [options] [模板选项]

Options:
  -n, --name <name>       正在创建的输出名称。如未指定名称，则使用输出目录的名称。
  -o, --output <output>   要放置生成的输出的位置。
  --dry-run               如果运行给定命令行将导致模板创建，则显示将发生情况的摘要。
  --force                 强制生成内容 (即使它会更改现有文件)。
  --no-update-check       在实例化模板时，禁用对模板包更新的检查。
  --project <project>     应用于上下文评估的项目。
  -lang, --language <C#>  指定要实例化的模板语言。
  --type <project>        指定要实例化的模板类型。

模板选项:
  -t, --param:type <param:type>  Set the silky host type, optional values: webhost, generalhost ,wshost, gateway
                                 类型: string
                                 默认: generalhost
  -do, --dockersupport           Add docker support for Silky
                                 类型: bool
                                 默认: true
  -r, --rpcport <rpcport>        Set the port for rpc listening
                                 类型: int
                                 默认: 2200
  -in, --infrastr                only include basic service orchestration files
                                 类型: bool
                                 默认: false
  -e, --env <env>                Set dotnet env
                                 类型: string
                                 默认: Development
  -m, --module                   Is it a module project
                                 类型: bool
                                 默认: false
  -p:i, --includeinfr            Whether to include the basic orchestration service.
                                 类型: bool
```

示例:

```powershell
# 创建网关
> dotnet new silky.app -t gateway -n Silky.Gateway

# 创建业务微服务
> dotnet new silky.app -t generalhost -n Silky.Demo
```