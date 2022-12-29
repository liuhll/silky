using System;
using Microsoft.Net.Http.Headers;

namespace Silky.Rpc.Transport.Messages;

public class SilkyFileContentResult
{
    public byte[] FileContents { get; set; }
    
    public string ContentType { get; set; }
    
    public string FileDownloadName { get; set; }
    
    public DateTimeOffset? LastModified { get; set; }
    
    public EntityTagHeaderValue? EntityTag { get; set; }
    
    public bool EnableRangeProcessing { get; set; }
    
}