namespace Lms.Rpc.Transport.CachingIntercept
{
    public interface ICacheKeyProvider 
    {
        public int Index { get; }
        

        string PropName  { get; internal set; }

        string Value { get; internal set; }
    }
}