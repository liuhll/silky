using System.Threading.Tasks;
using IAnotherApplication;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Silky.Transaction.Tcc;

namespace TestApplication.AppService;

public class TccTestAppService : ITccTestAppService
{
    private readonly IAnotherAppService _anotherAppService;

    public TccTestAppService(IAnotherAppService anotherAppService)
    {
        _anotherAppService = anotherAppService;
    }

    [TccTransaction(ConfirmMethod = "TestConfirm",CancelMethod = "TestCancel")]
    public async Task<string> Test(TestTccInput input)
    {
        await _anotherAppService.DeleteOne(input.Name);
        await _anotherAppService.DeleteTwo(input.Address);
        return "Tcc for Try";
    }
    
    public async Task<string> TestConfirm(TestTccInput input)
    {
        return "Tcc for Confirm";
    }
    
    public async Task<string> TestCancel(TestTccInput input)
    {
        return "Tcc for Cancel";
    }
}