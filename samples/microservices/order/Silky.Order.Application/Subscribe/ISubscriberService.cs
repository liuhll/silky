using System;

namespace Silky.Order.Application.Subscribe
{
    public interface ISubscriberService
    {
        void CheckReceivedMessage(DateTime datetime);
    }
}