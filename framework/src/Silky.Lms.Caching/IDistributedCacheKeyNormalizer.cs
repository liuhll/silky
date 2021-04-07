namespace Silky.Lms.Caching
{
    public interface IDistributedCacheKeyNormalizer 
    {
        string NormalizeKey(DistributedCacheKeyNormalizeArgs args);
    }
}