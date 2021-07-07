using System;

namespace Lms.Order.Application.Subscribe
{
    public interface ISubscriberService
    {
        void CheckReceivedMessage(DateTime datetime);
    }
}