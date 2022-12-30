using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ITestApplication.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestApplication.File;

public class FileAppService : IFileAppService
{
    public async Task PostFile(IFormFile file)
    {
        var filePath = Path.GetTempFileName();
        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);
    }

    public async Task PostFiles(IFormFileCollection files)
    {
        foreach (var file in files)
        {
            var filePath = Path.GetTempFileName();
            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);
        }
    }

    public async Task PostFormWithFile(FormWithFile formWithFile)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(),formWithFile.Name);
        await using var stream = System.IO.File.Create(filePath);
        await formWithFile.File.CopyToAsync(stream);
    }

    public async Task<IActionResult> Download()
    {
        var bytes = System.IO.File.ReadAllBytes("./Contents/测试.xlsx").ToArray();
        return new FileContentResult(bytes, "application/octet-stream")
        {
            FileDownloadName = "测试文件.xlsx",
        };
    }
}