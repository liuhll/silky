using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Lms.Core.Serialization;

namespace GatewayDemo.AppService
{
    public class GatewayTestAppService : ITestAppService
    {
        private readonly IJsonSerializer _serializer;

        public GatewayTestAppService(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<string> Create(TestDto input)
        {
            return _serializer.Serialize(input);
        }

        public async Task<string> Update(TestDto input)
        {
            return _serializer.Serialize(input);
        }

        public async Task<string> Search(TestDto query)
        {
            return _serializer.Serialize(query);
        }

        public async Task<string> Form(TestDto query)
        { 
            return _serializer.Serialize(query);
        }

        public async Task<string> Get(long id, string name)
        {
            return $"Id:{id} -- Name:{name}";
        }
    }
}