using System;
using System.Collections.Concurrent;

namespace Silky.Core
{
    public class BaseSingleton
    {
        static BaseSingleton()
        {
            AllSingletons = new ConcurrentDictionary<Type, object>();
        }

        public static ConcurrentDictionary<Type, object> AllSingletons { get; }
    }
}