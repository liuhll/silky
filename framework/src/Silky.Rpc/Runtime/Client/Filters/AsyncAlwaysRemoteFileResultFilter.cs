using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Silky.Core.Serialization;
using Silky.Rpc.Filters;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Client.Filters;

public class AsyncAlwaysRemoteFileResultFilter : IAsyncAlwaysRunClientResultFilter
{
    private readonly ISerializer _serializer;

    public AsyncAlwaysRemoteFileResultFilter(ISerializer serializer)
    {
        _serializer = serializer;
    }

    public async Task OnResultExecutionAsync(ClientResultExecutingContext context, ClientResultExecutionDelegate next)
    {
        if (context.Result?.IsFile == true)
        {
            var silkyFileContentResult = _serializer.Deserialize<SilkyFileContentResult>(context.Result.Result.ToString());
            var fileContentResult = new FileContentResult(silkyFileContentResult.FileContents,
                silkyFileContentResult.ContentType)
            {
                LastModified = silkyFileContentResult.LastModified,
                EntityTag = silkyFileContentResult.EntityTag,
                EnableRangeProcessing = silkyFileContentResult.EnableRangeProcessing,
                FileDownloadName = silkyFileContentResult.FileDownloadName,
            };
            
            context.Result.Result = fileContentResult;

        }
        await next();
    }
}