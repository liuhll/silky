using System.Threading.Tasks;
using IAnotherApplication;
using IAnotherApplication.Dtos;
using ITestApplication.Test;
using Silky.Transaction.Tcc;

namespace AnotherHostDemo.AppService
{
    public class AnotherAppService : IAnotherAppService
    {
        private readonly ITestAppService _testAppService;

        public AnotherAppService(ITestAppService testAppService)
        {
            _testAppService = testAppService;
        }


        [TccTransaction(ConfirmMethod = "DeleteOneConfirm", CancelMethod = "DeleteOneCancel")]
        public async Task<string> DeleteOne(string name)
        {
            //throw new BusinessException("DeleteOne exception");
            return "DeleteOne " + name;
        }

        [TccTransaction(ConfirmMethod = "DeleteTwoConfirm", CancelMethod = "DeleteTwoCancel")]
        public async Task<string> DeleteTwo(string address)
        {
            // throw new BusinessException("DeleteTwo exception");
            return "DeleteTwo " + address;
        }

        public async Task<string> Create(string name)
        
        
        
        {
            // await _testAppService.Create(new TestDto() { Name = name });
            return $"ok for {name}";
        }

        public async Task<TestDto> Test(TestDto input)
        {
            return input;
        }

        public void ReturnNullTest()
        {
        }

        public async Task<string> DeleteTwoConfirm(string name)
        {
            return "DeleteTwoConfirm " + name;
        }

        public async Task<string> DeleteTwoCancel(string name)
        {
            return "DeleteTwoCancel " + name;
        }

        public async Task<string> DeleteOneConfirm(string name)
        {
            return "DeleteConfirm " + name;
        }

        public async Task<string> DeleteOneCancel(string name)
        {
            return "DeleteCancel " + name;
        }
    }
}