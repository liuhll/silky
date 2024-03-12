namespace Silky.Rpc.Runtime.Server;

public class ResponseContextTypeAttribute : MetadataAttribute
{
    public ResponseContextTypeAttribute(string contentType) : base(ServiceConstant.ResponseContentTypeKey, contentType)
    {
    }
}