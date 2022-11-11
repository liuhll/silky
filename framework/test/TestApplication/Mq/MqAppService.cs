using System;
using System.Threading.Tasks;
using ITestApplication.Mq;
using ITestApplication.Mq.Message;
using MassTransit;

namespace TestApplication.Mq;

public class MqAppService : IMqAppService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IBus _bus;


    public MqAppService(IPublishEndpoint publishEndpoint,
        IBus bus)
    {
        _publishEndpoint = publishEndpoint;
        _bus = bus;
    }

    public async Task Publish(string data)
    {
        var mqData = new TestMessage()
        {
            Data = data
        };
        await _publishEndpoint.PublishForSilky(mqData, context =>
        {
            context.Headers.Set("test","test");
        });
    }

    public async Task Send(string data)
    {
        var mqData = new TestMessage()
        {
            Data = data
        };
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:events-listener"));
        await endpoint.SendForSilky(mqData, context =>
        {
            context.Headers.Set("test1","test1");
        });
    }
}