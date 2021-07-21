using System;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Silky.Order.Application.Subscribe
{
    public class SubscriberService : ICapSubscribe, ISubscriberService
    {
        private readonly ILogger<SubscriberService> _logger;

        public SubscriberService(ILogger<SubscriberService> logger)
        {
            _logger = logger;
        }

        [CapSubscribe("account.create.time")]
        public void CheckReceivedMessage(DateTime datetime)
        {
            _logger.LogInformation("Create Account Time:" + datetime.ToLocalTime());
        }
    }
}