using System.Diagnostics;
using System.Threading.Tasks;
using ITestApplication.Mq.Message;
using MassTransit;
using Silky.Core;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Core.Runtime.Session;
using Silky.EntityFrameworkCore.Repositories;
using Silky.MassTransit.Consumer;

namespace WsHostDemo.Mq.Consumer;

public class TestConsumer : SilkyConsumer<TestMessage>
{
    [UnitOfWork]
    protected async override Task ConsumeWork(ConsumeContext<TestMessage> context)
    {
        var loginUser = NullSession.Instance;
        Debug.Assert(loginUser.IsLogin(),"登录用户信息获取失败");
        var studentRepository = EngineContext.Current.Resolve<IRepository<Student>>();
        var student = new Student()
        {
            Name = context.Message.Data
        };
        await studentRepository.InsertAsync(student);
    }
}