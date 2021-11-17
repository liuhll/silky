using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silky.Core.Logging;
using Silky.RegistryCenter.Consul.Configuration;

namespace Silky.RegistryCenter.Consul
{
    public class ConsulHeartBeatService : IHeartBeatService
    {
        private Timer _heartBeatTimer;
        private readonly ConsulRegistryCenterOptions _registryCenterOptions;
        private readonly ILogger<ConsulHeartBeatService> _logger;


        public ConsulHeartBeatService(ILogger<ConsulHeartBeatService> logger,
            IOptionsMonitor<ConsulRegistryCenterOptions> registryCenterOptions)
        {
            _logger = logger;
            _registryCenterOptions = registryCenterOptions.CurrentValue;
        }

        public void Start(Func<Task> cacheServerFromConsul)
        {
            _heartBeatTimer = new Timer(HeartBeatCallBack, cacheServerFromConsul,
                TimeSpan.FromSeconds(_registryCenterOptions.HeartBeatInterval),
                TimeSpan.FromSeconds(_registryCenterOptions.HeartBeatInterval));
        }

        private async void HeartBeatCallBack(object state)
        {
            try 
            {
                Func<Task> cacheServerFromConsul = (Func<Task>)state;
                await cacheServerFromConsul();
            } catch (Exception ex) 
            {
                _logger.LogException(ex);
            }
            
        }


        public void Dispose()
        {
            _heartBeatTimer?.Dispose();
        }
    }
}