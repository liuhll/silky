using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Extensions;

public static class SilkyFormFileExtensions
{
    public static IFormFile ConventToFormFile(this SilkyFormFile silkyFile)
    {
        var formFile = new FormFile(
            new FileBufferingReadStream(new MemoryStream(silkyFile.Buffer), 65536), 0,
            silkyFile.Length,
            silkyFile.Name, silkyFile.FileName)
        {
            Headers = silkyFile.Headers ?? new HeaderDictionary(),
            ContentDisposition = silkyFile.ContentDisposition,
            ContentType = silkyFile.ContentType
        };
        return formFile;
    }
    
    public static IFormFileCollection ConventToFileCollection(this SilkyFormFile[] silkyFiles)
    {
        var formFiles = new List<IFormFile>();
        foreach (var silkyFile in silkyFiles)
        {
            var formFile = new FormFile(
                new FileBufferingReadStream(new MemoryStream(silkyFile.Buffer), 65536), 0,
                silkyFile.Length,
                silkyFile.Name, silkyFile.FileName)
            {
                Headers = silkyFile.Headers ?? new HeaderDictionary(),
                ContentDisposition = silkyFile.ContentDisposition,
                ContentType = silkyFile.ContentType
            };
            formFiles.Add(formFile);
        }

        var formFileCollection = new FormFileCollection();
        formFileCollection.AddRange(formFiles);
        return formFileCollection;
    }
    
    public static IFormFile[] ConventToFormFiles(this SilkyFormFile[] silkyFiles)
    {
        var formFiles = new List<IFormFile>();
        foreach (var silkyFile in silkyFiles)
        {
            var formFile = new FormFile(
                new FileBufferingReadStream(new MemoryStream(silkyFile.Buffer), 65536), 0,
                silkyFile.Length,
                silkyFile.Name, silkyFile.FileName)
            {
                Headers = silkyFile.Headers ?? new HeaderDictionary(),
                ContentDisposition = silkyFile.ContentDisposition,
                ContentType = silkyFile.ContentType
            };
            formFiles.Add(formFile);
        }

        return formFiles.ToArray();
    }
}