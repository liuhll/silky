using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;

namespace FileHostDemo.File;

[ServiceRoute]
// [Authorize]
public interface IFileAppService
{
    [HttpPost("single")]
    public Task PostFile(IFormFile file);

    [HttpPost("multiple")]
    public Task PostFiles(IFormFileCollection files);

    public Task PostFormWithFile(FormWithFile formWithFile);

    public Task<FileResult> Download();
}

public class FormWithFile
{
    public string Name { get; set; }

    public IFormFile File { get; set; }
}