using Silky.Transaction.Tcc;

namespace WebHostDemo.Hello;

public class HelloAppService : IHelloAppService
{
    public Task Exception()
    {
        throw new NotImplementedException();
    }

    [TccTransaction(ConfirmMethod = "TccTestConfirm", CancelMethod = "TccTestCancel")]
    public Task TccTest()
    {
        return Task.CompletedTask;
    }
    
    public Task TccTestConfirm()
    {
        return Task.CompletedTask;
    }
    
    public Task TccTestCancel()
    {
        return Task.CompletedTask;
    }
}