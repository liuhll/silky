using System.Threading.Tasks;
using IAnotherApplication;
using IAnotherApplication.Dtos;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Transaction.Tcc;

namespace TestApplication.AppService;

public class TccTestAppService : ITccTestAppService
{
    private readonly IAnotherAppService _anotherAppService;

    public TccTestAppService(IAnotherAppService anotherAppService)
    {
        _anotherAppService = anotherAppService;
    }

    [TccTransaction(ConfirmMethod = "TestConfirm", CancelMethod = "TestCancel")]
    [UnitOfWork]
    public async Task<string> Test(TestTccInput input)
    {
        var one = await _anotherAppService.DeleteOne(input.Name);
        var two = await _anotherAppService.DeleteTwo(input.Address);
        var test = await _anotherAppService.Test(new TestDto()
        {
            Name = input.Name,
            Address = input.Address
        });
        return "Tcc for Try";
    }

    [UnitOfWork]
    public async Task<string> TestConfirm(TestTccInput input)
    {
        return "Tcc for Confirm";
    }

    [UnitOfWork]
    public async Task<string> TestCancel(TestTccInput input)
    {
        return "Tcc for Cancel";
    }
}