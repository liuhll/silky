namespace Silky.Rpc.Runtime.Server;

public class CacheKeyProviderDescriptor
{
    public int Index { get; set; }
    public string PropName { get; set; }

    public int ParameterIndex { get; set; }

    public ParameterFrom From { get; set; }
    
    public bool IsSampleOrNullableType { get; set; }
}