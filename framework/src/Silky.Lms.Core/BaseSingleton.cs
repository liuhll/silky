using System;
using System.Collections.Generic;

namespace Silky.Lms.Core
{
    public class BaseSingleton
    {
        static BaseSingleton()
        {
            AllSingletons = new Dictionary<Type, object>();
        }
        
        public static IDictionary<Type, object> AllSingletons { get; }
    }
}