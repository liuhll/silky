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

        [TccTransaction(ConfirmMethod = "DeleteConfirm", CancelMethod = "DeleteCancel")]
        public async Task<string> Delete(string name)
        {
            await _testAppService.Delete(name);
            return "Tyring";
        }

        public async Task<string> DeleteConfirm(string name)
        {
            return "DeleteConfirm";
        }

        public async Task<string> DeleteCancel(string name)
        {
            return "DeleteCancel";
        }
    }
}