using Silky.Core;
using Demo.Application.Contracts.System;
using Demo.Application.Contracts.System.Dtos;
using Silky.Rpc.Endpoint;

namespace Demo.Application.System
{
    public class SystemAppService : ISystemAppService
    {
        public GetSystemInfoOutput GetInfo()
        {
            return new GetSystemInfoOutput()
            {
                HostName = EngineContext.Current.HostName,
                Environment = EngineContext.Current.HostEnvironment.EnvironmentName,
                Address = RpcEndpointHelper.GetLocalTcpEndpoint().ToString()
            };
        }
    }
}