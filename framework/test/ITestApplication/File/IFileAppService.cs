using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime;

namespace ITestApplication.File;

[ServiceRoute]
// [Authorize]
[HashShuntStrategy]
public interface IFileAppService
{
    [HttpPost("single")]
    public Task PostFile(IFormFile file);

    [HttpPost("multiple")]
    public Task PostFiles(IFormFileCollection files);

    public Task PostFormWithFile(FormWithFile formWithFile);

    public Task<IActionResult> Download();
}

public class FormWithFile
{
    public string Name { get; set; }

    public IFormFile File { get; set; }
}