using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Silky.Core.Extensions;

namespace Silky.Core.Configuration;

public static class ConfigurationHelper
{
    public static IConfigurationRoot BuildConfiguration(
        IConfigurationBuilder builder,
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

        builder
            .SetBasePath(options.BasePath).SetBasePath(options.BasePath);

        switch (options.FileType)
        {
            case ConfigurationFileType.Yaml:
                builder
                    .AddYamlFile(options.FileName + ".yaml", optional: options.Optional,
                        reloadOnChange: options.ReloadOnChange);
                if (!options.EnvironmentName.IsNullOrEmpty())
                {
                    builder.AddYamlFile($"{options.FileName}.{options.EnvironmentName}.yaml",
                        optional: options.Optional, reloadOnChange: options.ReloadOnChange);
                }
                break;
            case ConfigurationFileType.Yml:
                builder
                    .AddYamlFile(options.FileName + ".yml", optional: options.Optional,
                        reloadOnChange: options.ReloadOnChange);
                if (!options.EnvironmentName.IsNullOrEmpty())
                {
                    builder.AddYamlFile($"{options.FileName}.{options.EnvironmentName}.yml",
                        optional: options.Optional, reloadOnChange: options.ReloadOnChange);
                }
                break;
            case ConfigurationFileType.Json:
                builder
                    .AddJsonFile(options.FileName + ".json", optional: options.Optional,
                        reloadOnChange: options.ReloadOnChange);
                if (!options.EnvironmentName.IsNullOrEmpty())
                {
                    builder.AddJsonFile($"{options.FileName}.{options.EnvironmentName}.json",
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

        builder.AddEnvironmentVariables(options.EnvironmentVariablesPrefix);
        if (options.CommandLineArgs != null)
        {
            builder.AddCommandLine(options.CommandLineArgs);
        }

        builderAction?.Invoke(builder);
        return builder.Build();
    }
}