using Silky.Core;
using SilkyApp.Application.Contracts.System;
using SilkyApp.Application.Contracts.System.Dtos;

namespace SilkyApp.Application.System
{
    public class SystemAppService : ISystemAppService
    {
        public GetSystemInfoOutput GetInfo()
        {
            return new GetSystemInfoOutput()
            {
                HostName = EngineContext.Current.HostName,
                AppName = EngineContext.Current.AppName,
                Environment = EngineContext.Current.HostEnvironment.EnvironmentName,
                
            };
        }
    }
}