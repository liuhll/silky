using System.Threading.Tasks;
using ITestApplication.Test;
using Lms.Transaction.Tcc;

namespace GatewayDemo.AppService
{
    public class TransTestAppService : ITransTestAppService
    {
        private readonly ITestAppService _testAppService;

        public TransTestAppService(ITestAppService testAppService)
        {
            _testAppService = testAppService;
        }

        [TccTransaction(ConfirmMethod = "CreateConfirm", CancelMethod = "CreateCancel")]
        public async Task<string> Create(string name)
        {
            await _testAppService.Delete(name);
            return "Tyring";
        }
        
        public async Task<string> CreateConfirm(string name)
        {
            return "CreateConfirm";
        }
        
        public async Task<string> CreateCancel(string name)
        {
            return "CancelConfirm";
        }
    }
}