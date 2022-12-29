using Microsoft.AspNetCore.Http;

namespace Silky.Rpc.Transport.Messages;

public class SilkyFormFile
{
    public byte[] Buffer { get; set; }

    public string ContentType { get; set; }
    
    public string ContentDisposition { get; set; }
    
    public HeaderDictionary Headers { get; set; }
    
    public long Length { get; set; }
    
    public string Name { get; set; }
    
    public string FileName { get; set; }
}