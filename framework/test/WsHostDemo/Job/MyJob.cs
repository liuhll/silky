using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silky.Rpc.Runtime.Client;
using Sundial;

namespace WsHostDemo.Job;

public class MyJob : IJob
{
    private readonly ILogger<MyJob> _logger;
    private readonly IInvokeTemplate _invokeTemplate;
    
    public MyJob(ILogger<MyJob> logger, IInvokeTemplate invokeTemplate)
    {
        _logger = logger;
        _invokeTemplate = invokeTemplate;
    }
    
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{context}");
         await _invokeTemplate.PostAsync("api/test", new { Name = "Liuhll", Address = "Beijing" });
    }
}