namespace Silky.Rpc.Runtime.Server
{
    public interface ICacheKeyProvider
    {
        public int Index { get; }

        string PropName { get; set; }
        
    }
}