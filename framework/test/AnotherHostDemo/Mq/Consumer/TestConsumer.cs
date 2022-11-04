using System.Diagnostics;
using System.Threading.Tasks;
using ITestApplication.Mq.Message;
using MassTransit;
using Silky.Core.Runtime.Session;
using Silky.MassTransit.Consumer;

namespace AnotherHostDemo.Mq.Consumer;

public class TestConsumer : SilkyConsumer<TestMessage>
{
    protected async override Task ConsumeWork(ConsumeContext<TestMessage> context)
    {
        var loginUser = NullSession.Instance;
        Debug.Assert(loginUser.IsLogin(),"登录用户信息获取失败");
    }
}