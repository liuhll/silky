using System.Threading.Tasks;
using IAnotherApplication;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
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
        public async Task<string> DeleteOne(string name)
        {
            //throw new BusinessException("测试异常");
            return "DeleteOne " + name;
        }

        [TccTransaction(ConfirmMethod = "DeleteTwoConfirm", CancelMethod = "DeleteTwoCancel")]
        public async Task<string> DeleteTwo(string name)
        {
            // throw new BusinessException("测试异常");
            return "DeleteTwo " + name;
        }

        public async Task<string> Create(string name)
        {
            await _testAppService.Create(new TestInput() {Name = name});
            return "ok";
        }

        public async Task<string> DeleteTwoConfirm(string name)
        {
            return "DeleteTwoConfirm " + name;
        }

        public async Task<string> DeleteTwoCancel(string name)
        {
            return "DeleteTwoCancel " + name;
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