namespace Silky.Rpc.Runtime.Client
{
    public enum FailoverType
    {
        Communication = 0,
        
        NonSilkyFrameworkException,
        
        UserDefined,
        
        Other,
    }
}