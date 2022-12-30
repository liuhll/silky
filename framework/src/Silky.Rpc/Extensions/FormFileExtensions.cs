using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Extensions;

public static class FormFileExtensions
{
    public static SilkyFormFile ConventToSilkyFile(this IFormFile file)
    {
        var buffer = new byte[file.Length];
        using var steam =  file.OpenReadStream();
        _ = steam.Read(buffer);
        var silkyFile = new SilkyFormFile()
        {
            FileName = file.FileName,
            Headers = (HeaderDictionary)file.Headers,
            Name = file.Name,
            Length = file.Length,
            Buffer = buffer,
            ContentDisposition = file.ContentDisposition,
            ContentType = file.ContentType
        };
        return silkyFile;
    }
    
    public static SilkyFormFile[] ConventToSilkyFiles(this IFormFileCollection files)
    {
        var silkyFiles = new List<SilkyFormFile>();
        foreach (var file in files)
        {
            var buffer = new byte[file.Length];
            using var steam =  file.OpenReadStream();
            _ = steam.Read(buffer);
            var silkyFile = new SilkyFormFile()
            {
                FileName = file.FileName,
                Headers = (HeaderDictionary)file.Headers,
                Name = file.Name,
                Length = file.Length,
                Buffer = buffer,
                ContentDisposition = file.ContentDisposition,
                ContentType = file.ContentType
            };
            silkyFiles.Add(silkyFile);
        }
       
        return silkyFiles.ToArray();
    }
}