using System.Threading.Tasks;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Silky.Core.Exceptions;
using Silky.Transaction.Tcc;

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
        public async Task<string> Delete(TestInput input)
        {
            await _testAppService.Delete(input);
            //throw new BusinessException("test error");
            return "Tyring";
        }

        public async Task<string> DeleteConfirm(TestInput input)
        {
            return "DeleteConfirm";
        }

        public async Task<string> DeleteCancel(TestInput input)
        {
            return "DeleteCancel";
        }
    }
}