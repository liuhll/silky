namespace Silky.Rpc.Runtime.Server;

public class IgnoreBodyDataAttribute : MetadataAttribute
{
    public IgnoreBodyDataAttribute() : base(ServiceEntryConstant.IgnoreBodyData, true)
    {
    }
}