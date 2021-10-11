using System.Threading.Tasks;
using ITestApplication.Test.Dtos;
using ITestApplication.Test.Fallback;
using Microsoft.Extensions.Logging;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Core.DependencyInjection;
using Silky.Core.Serialization;

namespace NormHostDemo.AppService.Fallback
{
    public class CreateFallbackService : ICreateFallback, IScopedDependency
    {
        private readonly ISerializer _serializer;
        private readonly ILogger<CreateFallbackService> _logger;

        public CreateFallbackService(ISerializer serializer,
            ILogger<CreateFallbackService> logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        [UnitOfWork]
        public async Task<TestOut> Create(TestInput input)
        {
            _logger.LogDebug("fallback:" + _serializer.Serialize(input));
            return new TestOut()
            {
                Name = "fallback" + input.Name,
                Address = "fallback" + input.Name
            };
        }
    }
}