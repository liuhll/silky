using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Silky.Rpc.SkyApm.Configuration;
using SkyApm;
using SkyApm.Utilities.Configuration;

namespace Silky.SkyApm.Agent.Configuration
{
    public class SilkyConfigurationFactory : IConfigurationFactory
    {
        private const string CONFIG_FILE_PATH_COMPATIBLE = "SKYWALKING__CONFIG__PATH";
        private const string CONFIG_FILE_PATH = "SKYAPM__CONFIG__PATH";
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly IEnumerable<IAdditionalConfigurationSource> _additionalConfigurations;
        private readonly IConfiguration _configuration;

        public SilkyConfigurationFactory(IEnvironmentProvider environmentProvider,
            IEnumerable<IAdditionalConfigurationSource> additionalConfigurations,
            IConfiguration configuration)
        {
            _environmentProvider = environmentProvider;
            _additionalConfigurations = additionalConfigurations;
            _configuration = configuration;
        }

        public IConfiguration Create()
        {
            var builder = new ConfigurationBuilder();

            builder.AddSkyWalkingDefaultConfig(_configuration);

            builder.AddJsonFile("appsettings.json", true)
                .AddJsonFile($"appsettings.{_environmentProvider.EnvironmentName}.json", true);

            builder.AddJsonFile("skywalking.json", true)
                .AddJsonFile($"skywalking.{_environmentProvider.EnvironmentName}.json", true);

            builder.AddJsonFile("skyapm.json", true)
                .AddJsonFile($"skyapm.{_environmentProvider.EnvironmentName}.json", true);

            builder.AddYamlFile("appsettings.yml", optional: true)
                .AddYamlFile($"appsettings.{_environmentProvider.EnvironmentName}.yml", optional: true);

            builder.AddYamlFile("appsettings.yaml", optional: true)
                .AddYamlFile($"appsettings.{_environmentProvider.EnvironmentName}.yaml", optional: true);

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH_COMPATIBLE)))
            {
                builder.AddJsonFile(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH_COMPATIBLE), false);
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH)))
            {
                builder.AddJsonFile(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH), false);
            }

            builder.AddEnvironmentVariables();

            if (_configuration != null)
            {
                builder.AddConfiguration(_configuration);
            }

            foreach (var additionalConfiguration in _additionalConfigurations)
            {
                additionalConfiguration.Load(builder);
            }

            return builder.Build();
        }
    }
}