using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;

namespace GatewayDemo.File;

[ServiceRoute]
// [Authorize]
public interface IFileAppService
{
    [HttpPost("single")]
    public Task PostFile([FromForm] IFormFile file);

    [HttpPost("multiple")]
    public Task PostFiles([FromForm] IFormFileCollection files);

    public Task PostFormWithFile([FromForm] FormWithFile formWithFile);

    public Task<FileResult> Download();
}

public class FormWithFile
{
    public string Name { get; set; }

    public IFormFile File { get; set; }
}