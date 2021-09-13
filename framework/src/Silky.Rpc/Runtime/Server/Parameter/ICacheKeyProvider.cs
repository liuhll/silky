namespace Silky.Rpc.Runtime.Server.Parameter
{
    public interface ICacheKeyProvider
    {
        public int Index { get; }

        string PropName { get; set; }

        string Value { get; set; }
    }
}