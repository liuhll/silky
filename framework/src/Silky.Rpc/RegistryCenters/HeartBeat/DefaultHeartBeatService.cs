using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Logging;

namespace Silky.Rpc.RegistryCenters.HeartBeat
{
    public class DefaultHeartBeatService : IHeartBeatService
    {
        private Timer _heartBeatTimer;
        private readonly ILogger<DefaultHeartBeatService> _logger;

        public DefaultHeartBeatService(ILogger<DefaultHeartBeatService> logger)
        {
            _logger = logger;
        }

        public void Start(Func<Task> funcTask)
        {
            var heartBeatInterval = GetHeartBeatInterval();
            _heartBeatTimer = new Timer(HeartBeatCallBack, funcTask,
                TimeSpan.FromSeconds(heartBeatInterval),
                TimeSpan.FromSeconds(heartBeatInterval));
        }

        private double GetHeartBeatInterval()
        {
            if (EngineContext.Current.Configuration.GetSection("registrycenter:heartbeatintervalsecond").Exists())
            {
                var heartBeatInterval =
                    EngineContext.Current.Configuration.GetValue<int>("registrycenter:heartbeatintervalsecond");

                return heartBeatInterval;
            }
            return 10;
        }

        private async void HeartBeatCallBack(object state)
        {
            try
            {
                Func<Task> funcTask = (Func<Task>)state;
                await funcTask();
            }
            catch (Exception ex)
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