namespace Lms.Rpc.Transport.CachingIntercept
{
    public interface ICacheKeyProvider 
    {
        public int Order { get; }

        public string Name { get; set; }

        string PropName  { get; internal set; }
    }
}