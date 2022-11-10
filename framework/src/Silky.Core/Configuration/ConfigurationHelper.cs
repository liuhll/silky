using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Silky.Core.Extensions;

namespace Silky.Core.Configuration;

public static class ConfigurationHelper
{
    public static IConfigurationRoot BuildConfiguration(
        IHostEnvironment hostEnvironment,
        SilkyConfigurationBuilderOptions? options = null,
        Action<IConfigurationBuilder> builderAction = null)
    {
        options ??= new SilkyConfigurationBuilderOptions();

        if (options.BasePath.IsNullOrEmpty())
        {
            options.BasePath = hostEnvironment.ContentRootPath;
        }

        if (options.EnvironmentName.IsNullOrEmpty())
        {
            options.EnvironmentName = hostEnvironment.EnvironmentName;
        }

        if (options.BasePath.IsNullOrEmpty())
        {
            options.BasePath = Directory.GetCurrentDirectory();
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(options.BasePath).SetBasePath(options.BasePath);

        switch (options.FileType)
        {
            case ConfigurationFileType.Yaml:
                builder = builder
                    .AddYamlFile(options.FileName + ".yaml", optional: options.Optional,
                        reloadOnChange: options.ReloadOnChange);
                if (!options.EnvironmentName.IsNullOrEmpty())
                {
                    builder.AddYamlFile($"{options.FileName}.{options.EnvironmentName}.yaml",
                        optional: options.Optional, reloadOnChange: options.ReloadOnChange);
                }

                break;
            case ConfigurationFileType.Json:
                builder = builder
                    .AddJsonFile(options.FileName + ".json", optional: options.Optional,
                        reloadOnChange: options.ReloadOnChange);
                if (!options.EnvironmentName.IsNullOrEmpty())
                {
                    builder.AddYamlFile($"{options.FileName}.{options.EnvironmentName}.json",
                        optional: options.Optional, reloadOnChange: options.ReloadOnChange);
                }

                break;
        }

        if (options.EnvironmentName == "Development")
        {
            if (options.UserSecretsId != null)
            {
                builder.AddUserSecrets(options.UserSecretsId);
            }
            else if (options.UserSecretsAssembly != null)
            {
                builder.AddUserSecrets(options.UserSecretsAssembly, true);
            }
        }

        builder = builder.AddEnvironmentVariables(options.EnvironmentVariablesPrefix);
        builder = builder.AddEnvironmentVariables();
        if (options.CommandLineArgs != null)
        {
            builder = builder.AddCommandLine(options.CommandLineArgs);
        }

        builderAction?.Invoke(builder);
        return builder.Build();
    }
}