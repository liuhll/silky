using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Utils;
using Silky.Rpc.Runtime.Server;
using Silky.Swagger.Abstraction;
using Silky.Swagger.Abstraction.SwaggerGen.DependencyInjection;
using Silky.Swagger.Abstraction.SwaggerGen.Filters;
using Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator;
using Silky.Swagger.Gen.Configuration;
using Silky.Swagger.Gen.Provider;
using Silky.Swagger.Gen.Provider.Consul;
using Silky.Swagger.Gen.Provider.Nacos;
using Silky.Swagger.Gen.Provider.Zookeeper;
using Silky.Swagger.Gen.Register;
using Silky.Swagger.Gen.Register.Consul;
using Silky.Swagger.Gen.Register.Nacos;
using Silky.Swagger.Gen.Register.Zookeeper;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddSwaggerInfoService(this IServiceCollection services, string registerType)
    {
        switch (registerType?.ToLower())
        {
            case "zookeeper":
                services.AddSingleton<ISwaggerInfoRegister, ZookeeperSwaggerInfoRegister>();
                services.AddSingleton<ISwaggerInfoProvider, ZookeeperSwaggerInfoProvider>();
                services.AddScoped<IRegisterCenterSwaggerInfoProvider, ZookeeperSwaggerInfoProvider>();
                break;
            case "nacos":
                services.AddSingleton<ISwaggerInfoRegister, NacosSwaggerInfoRegister>();
                services.AddSingleton<ISwaggerInfoProvider, NacosSwaggerInfoProvider>();
                services.AddScoped<IRegisterCenterSwaggerInfoProvider, NacosSwaggerInfoProvider>();
                break;
            case "consul":
                services.AddSingleton<ISwaggerInfoRegister, ConsulSwaggerInfoRegister>();
                services.AddSingleton<ISwaggerInfoProvider, ConsulSwaggerInfoProvider>();
                services.AddScoped<IRegisterCenterSwaggerInfoProvider, ConsulSwaggerInfoProvider>();
                break;
            default:
                if (!EngineContext.Current.ApplicationOptions.IgnoreCheckingRegisterType)
                {
                    if (registerType.IsNullOrEmpty())
                    {
                        throw new SilkyException("You did not specify the service registry type");
                    }

                    throw new SilkyException(
                        $"The system does not provide a service registration center of type {registerType}");
                }

                break;
        }

        return services;
    }

    public static void AddSwaggerInfoService(this IServiceCollection services, IConfiguration configuration)
    {
        var registerType = configuration.GetValue<string>("registrycenter:type");

        if (!services.IsAdded(typeof(ISwaggerInfoRegister)) || !services.IsAdded(typeof(ISwaggerInfoProvider)))
        {
            services.AddSwaggerInfoService(registerType);
        }
    }

    public static void AddSwaggerRegisterInfoGen(this IServiceCollection services, IConfiguration configuration)
    {
        if (!services.IsAdded(typeof(ISwaggerProvider)))
        {
            services.AddOptions<SilkySwaggerGenOptions>()
                .Bind(EngineContext.Current.Configuration.GetSection(SilkySwaggerGenOptions.SwaggerDocument));
            var swaggerGenOption = configuration.GetSection(SilkySwaggerGenOptions.SwaggerDocument)
                .Get<SilkySwaggerGenOptions>();
            services.AddSwaggerGen(options =>
            {
                var groups = SwaggerGroupUtils.ReadGroupInfos(ServiceHelper.ReadInterfaceAssemblies());
                foreach (var group in groups)
                {
                    options.SwaggerDoc(group.Item1,
                        new OpenApiInfo()
                        {
                            Title = swaggerGenOption.Title ?? group.Item1,
                            Version = swaggerGenOption.Version,
                            Description = swaggerGenOption.Description,
                            License = swaggerGenOption.License,
                            TermsOfService = swaggerGenOption.TermsOfService,
                            Contact = swaggerGenOption.Contact,
                        });
                }

                if (swaggerGenOption.EnableHashRouteHeader)
                {
                    options.AddHashRouteHeader();
                }

                if (swaggerGenOption.EnableMultipleServiceKey)
                {
                    options.MultipleServiceKey();
                }

                foreach (var filterType in swaggerGenOption.GetFilterTypes())
                {
                    options.OperationFilterDescriptors.Add(new FilterDescriptor()
                    {
                        Type = filterType,
                        Arguments = Array.Empty<object>()
                    });
                }
                
                options.SchemaFilter<EnumSchemaFilter>();

                LoadXmlComments(options);
            });
        }
    }

    private static void LoadXmlComments(SwaggerGenOptions swaggerGenOptions)
    {
        var projectAssemblies = EngineContext.Current.TypeFinder.GetAssemblies();
        foreach (var projectAssembly in projectAssemblies)
        {
            var xmlFile = $"{projectAssembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                swaggerGenOptions.IncludeXmlComments(xmlPath);
            }
        }
    }
}