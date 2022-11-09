using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestApplication.Contexts;

namespace TestApplication;

public class TestApplicationConfigureService : IConfigureService
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseAccessor(options => { options.AddDbPool<DemoDbContext>(); }, "TestApplication");
    }
}