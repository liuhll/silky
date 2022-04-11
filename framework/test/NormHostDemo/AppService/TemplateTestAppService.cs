using System.Collections.Generic;
using System.Threading.Tasks;
using ITestApplication.Test;
using Silky.Rpc.Runtime.Client;

namespace NormHostDemo.AppService;

public class TemplateTestAppService : ITemplateTestAppService
{
    private readonly IInvokeTemplate _invokeTemplate;

    public TemplateTestAppService(IInvokeTemplate invokeTemplate)
    {
        _invokeTemplate = invokeTemplate;
    }

    // public async Task<string> CallDeleteOne()
    // {
    //     var result = await _invokeTemplate.DeleteForObjectAsync<string>("/api/another/{name}", "test");
    //     return result;
    // }

    public async Task<string> CallCreateOne(string name)
    {
        var result = await _invokeTemplate.PostForObjectAsync<string>("api/another/{name}", name);
        return result;
    }

    public async Task<dynamic> CallTest()
    {
        // 1. 参数顺序与提供者一致
        // var result =
        //     await _invokeTemplate.PostForObjectAsync<dynamic>("api/another/test",
        //         new { Name = "张三", Address = "beijing" });

        // 2. 使用字典传递参数
        var result =
            await _invokeTemplate.PostForObjectAsync<dynamic>("api/another/test", new Dictionary<string, object>()
            {
                { "input", new { Name = "张三", Address = "beijing" } }
            });
        return result;
    }
}