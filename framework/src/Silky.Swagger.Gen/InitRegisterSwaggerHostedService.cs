using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Silky.Swagger.Gen.Register;

namespace Silky.Swagger.Gen;

public class InitRegisterSwaggerHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ISwaggerInfoRegister _swaggerInfoRegister;

    public InitRegisterSwaggerHostedService(IHostApplicationLifetime hostApplicationLifetime,
        ISwaggerInfoRegister swaggerInfoRegister)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _swaggerInfoRegister = swaggerInfoRegister;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(async () => { await _swaggerInfoRegister.Register(); });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}