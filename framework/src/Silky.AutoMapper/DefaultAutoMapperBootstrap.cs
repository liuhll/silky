using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Silky.Core;
using Microsoft.Extensions.Logging;

namespace Silky.AutoMapper
{
    public class DefaultAutoMapperBootstrap : IAutoMapperBootstrap
    {
        private readonly ILogger<DefaultAutoMapperBootstrap> _logger;
        
        public DefaultAutoMapperBootstrap(ILogger<DefaultAutoMapperBootstrap> logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                var profileTypes = GetProfileTypes();
                
                foreach (var profileType in profileTypes)
                {
                    cfg.AddProfile(profileType);
                }
            });
            AutoMapperConfiguration.Init(config);
        }

        private IEnumerable<Type> GetProfileTypes()
        {
            return EngineContext.Current.TypeFinder.FindClassesOfType<Profile>().Where(p=> !p.IsAbstract);
        }
    }
}