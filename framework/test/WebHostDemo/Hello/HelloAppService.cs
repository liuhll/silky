namespace WebHostDemo.Hello;

public class HelloAppService : IHelloAppService
{
    public Task Exception()
    {
        throw new NotImplementedException();
    }
}