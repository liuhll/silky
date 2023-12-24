using System.Threading.Tasks;
using IAnotherApplication;
using IAnotherApplication.Dtos;
using ITestApplication.Test;
using ITestApplication.Test.Dtos;
using NormHostDemo.Tests;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.EntityFrameworkCore.Repositories;
using Silky.Transaction.Tcc;

namespace TestApplication.AppService;

public class TccTestAppService : ITccTestAppService
{
    private readonly IAnotherAppService _anotherAppService;
    private readonly IRepository<Test> _testRepository;

    public TccTestAppService(IAnotherAppService anotherAppService, IRepository<Test> testRepository)
    {
        _anotherAppService = anotherAppService;
        _testRepository = testRepository;
    }

    [TccTransaction(ConfirmMethod = "TestConfirm", CancelMethod = "TestCancel")]
    [UnitOfWork]
    public async Task<string> Test(TestTccInput input)
    {
        // var one = await _anotherAppService.DeleteOne(input.Name);
        // var two = await _anotherAppService.DeleteTwo(input.Address);
        // var test = await _anotherAppService.Test(new TestDto()
        // {
        //     Name = input.Name,
        //     Address = input.Address
        // });
       var result = await _testRepository.InsertNowAsync(new Test()
        {
            Name = input.Name,
            Address = input.Address
        });
        RpcContext.Context.SetTransAttachment("Id",result.Entity.Id);
        return "Tcc for Try";
    }

    [UnitOfWork]
    public async Task<string> TestConfirm(TestTccInput input)
    {
        var id = RpcContext.Context.GetTransAttachment("Id").To<long>();
        var entity = await _testRepository.FindAsync(id);
        entity.Address += "Confirm";
        await _testRepository.UpdateNowAsync(entity);
        return "Tcc for Confirm";
    }

    [UnitOfWork]
    public async Task<string> TestCancel(TestTccInput input)
    {
        return "Tcc for Cancel";
    }
}