using System.Threading.Tasks;
using IAnotherApplication;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Lms.Core.Exceptions;
using Lms.Transaction.Tcc;

namespace AnotherHostDemo.AppService
{
    public class AnotherAppService : IAnotherAppService
    {
        private readonly ITestAppService _testAppService;

        public AnotherAppService(ITestAppService testAppService)
        {
            _testAppService = testAppService;
        }


        [TccTransaction(ConfirmMethod = "DeleteConfirm", CancelMethod = "DeleteCancel")]
        public async Task<string> Delete(string name)
        {
            //throw new BusinessException("测试异常");
            return "Delete " + name;
        }

        public async Task<string> Create(string name)
        {
            await _testAppService.Create(new TestInput() {Name = name});
            return "ok";
        }

        public async Task<string> DeleteConfirm(string name)
        {
            return "DeleteConfirm " + name;
        }

        public async Task<string> DeleteCancel(string name)
        {
            return "DeleteCancel " + name;
        }
    }
}