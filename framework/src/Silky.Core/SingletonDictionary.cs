using System.Collections.Concurrent;

namespace Silky.Core
{
    public class SingletonDictionary<TKey, TValue> : Singleton<ConcurrentDictionary<TKey, TValue>>
    {
        static SingletonDictionary()
        {
            Singleton<ConcurrentDictionary<TKey, TValue>>.Instance = new ConcurrentDictionary<TKey, TValue>();
        }

        public static new ConcurrentDictionary<TKey, TValue> Instance =>
            Singleton<ConcurrentDictionary<TKey, TValue>>.Instance;
    }
}